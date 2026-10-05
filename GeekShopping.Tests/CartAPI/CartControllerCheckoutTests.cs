using System.Security.Claims;
using GeekShopping.CartAPI.Controllers;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using GeekShopping.MessageBus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GeekShopping.Tests.CartAPI
{
    public class CartControllerCheckoutTests
    {
        private const string UserId = "user-1";
        private const string AccessToken = "access-token";

        private readonly Mock<ICartRepository> _cartRepository = new();
        private readonly Mock<ICouponRepository> _couponRepository = new();
        private readonly Mock<IRabbitMQMessageSender> _messageSender = new();

        // 50.00 × 2 + 30.00 × 1 = 130.00
        private static readonly CartDetailVO[] Items =
        {
            new CartDetailVO { Id = 1, ProductId = 1, Count = 2, Product = new ProductVO { Id = 1, Name = "T-shirt", Price = 50.00m } },
            new CartDetailVO { Id = 2, ProductId = 2, Count = 1, Product = new ProductVO { Id = 2, Name = "Mug", Price = 30.00m } }
        };

        public CartControllerCheckoutTests()
        {
            SetCart(Items);
            _couponRepository.Setup(r => r.GetCouponByCouponCode("GEEK10", AccessToken))
                .ReturnsAsync(new CouponVO { CouponCode = "GEEK10", DiscountAmount = 10 });
            _couponRepository.Setup(r => r.GetCouponByCouponCode("BIG500", AccessToken))
                .ReturnsAsync(new CouponVO { CouponCode = "BIG500", DiscountAmount = 500 });
            // What CouponRepository returns when the coupon API answers 404
            _couponRepository.Setup(r => r.GetCouponByCouponCode("FAKE", AccessToken))
                .ReturnsAsync(new CouponVO());
        }

        private void SetCart(IEnumerable<CartDetailVO> items) =>
            _cartRepository.Setup(r => r.FindCartByUserId(UserId)).ReturnsAsync(new CartVO
            {
                CartHeader = new CartHeaderVO { Id = items.Any() ? 10 : 0, UserId = UserId },
                CartDetails = items
            });

        // The controller reads the caller's access token through HttpContext.GetTokenAsync,
        // which asks the registered IAuthenticationService for the stored tokens.
        private CartController CreateController()
        {
            var properties = new AuthenticationProperties();
            properties.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = AccessToken } });
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity("Bearer")), properties, "Bearer");

            var authentication = new Mock<IAuthenticationService>();
            authentication
                .Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(ticket));

            var services = new ServiceCollection().AddSingleton(authentication.Object).BuildServiceProvider();

            return new CartController(_cartRepository.Object, _messageSender.Object, _couponRepository.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } }
            };
        }

        private void VerifyNothingWasOrdered()
        {
            _messageSender.Verify(s => s.SendMessage(It.IsAny<BaseMessage>(), It.IsAny<string>()), Times.Never);
            _cartRepository.Verify(r => r.ClearCart(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Checkout_PublishesTheOrderAndClearsTheCart()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId });

            var order = Assert.IsType<CheckoutHeaderVO>(Assert.IsType<OkObjectResult>(result.Result).Value);
            Assert.Equal(Items, order.CartDetails);
            Assert.Equal(130.00m, order.PurchaseAmount);
            Assert.Equal(0m, order.DiscountTotal);
            Assert.NotNull(order.DateTime);
            _messageSender.Verify(s => s.SendMessage(order, "checkoutqueue"), Times.Once);
            _cartRepository.Verify(r => r.ClearCart(UserId), Times.Once);
        }

        [Theory]
        [InlineData("1")]        // a client paying 1 for a 130 cart
        [InlineData("0")]
        [InlineData("-50")]
        [InlineData("99999")]
        public async Task Checkout_IgnoresTheAmountSentByTheClient(string sentAmount)
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO
            {
                UserId = UserId,
                PurchaseAmount = decimal.Parse(sentAmount)
            });

            var order = Assert.IsType<CheckoutHeaderVO>(Assert.IsType<OkObjectResult>(result.Result).Value);
            Assert.Equal(130.00m, order.PurchaseAmount);
        }

        [Fact]
        public async Task Checkout_DiscountWithoutACoupon_IsIgnored()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId, DiscountTotal = 100 });

            var order = Assert.IsType<CheckoutHeaderVO>(Assert.IsType<OkObjectResult>(result.Result).Value);
            Assert.Equal(0m, order.DiscountTotal);
            Assert.Equal(130.00m, order.PurchaseAmount);
        }

        [Fact]
        public async Task Checkout_WithMatchingCoupon_ChargesTheTotalMinusTheDiscount()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId, CouponCode = "GEEK10", DiscountTotal = 10 });

            var order = Assert.IsType<CheckoutHeaderVO>(Assert.IsType<OkObjectResult>(result.Result).Value);
            Assert.Equal(120.00m, order.PurchaseAmount);
            Assert.Equal(10m, order.DiscountTotal);
            _couponRepository.Verify(r => r.GetCouponByCouponCode("GEEK10", AccessToken), Times.Once);
        }

        [Fact]
        public async Task Checkout_CouponWorthMoreThanTheCart_ChargesZero()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId, CouponCode = "BIG500", DiscountTotal = 500 });

            var order = Assert.IsType<CheckoutHeaderVO>(Assert.IsType<OkObjectResult>(result.Result).Value);
            Assert.Equal(0m, order.PurchaseAmount);
        }

        [Fact]
        public async Task Checkout_WithDiscountThatDoesNotMatchTheCoupon_Returns412AndKeepsTheCart()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId, CouponCode = "GEEK10", DiscountTotal = 50 });

            Assert.Equal(412, Assert.IsType<StatusCodeResult>(result.Result).StatusCode);
            VerifyNothingWasOrdered();
        }

        [Theory]
        [InlineData(null)]    // a made-up code with no discount used to pass the check
        [InlineData("10")]
        public async Task Checkout_WithUnknownCoupon_Returns412(string? sentDiscount)
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO
            {
                UserId = UserId,
                CouponCode = "FAKE",
                DiscountTotal = sentDiscount == null ? null : decimal.Parse(sentDiscount)
            });

            Assert.Equal(412, Assert.IsType<StatusCodeResult>(result.Result).StatusCode);
            VerifyNothingWasOrdered();
        }

        [Fact]
        public async Task Checkout_EmptyCart_Returns400AndPublishesNothing()
        {
            SetCart(Array.Empty<CartDetailVO>());

            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId });

            Assert.IsType<BadRequestObjectResult>(result.Result);
            VerifyNothingWasOrdered();
        }

        [Fact]
        public async Task Checkout_WithoutUser_ReturnsBadRequest()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = null! });

            Assert.IsType<BadRequestResult>(result.Result);
            VerifyNothingWasOrdered();
        }
    }
}
