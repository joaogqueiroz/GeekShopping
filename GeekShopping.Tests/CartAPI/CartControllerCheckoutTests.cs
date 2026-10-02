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

        private static readonly CartDetailVO[] Items =
        {
            new CartDetailVO { ProductId = 1, Count = 2 },
            new CartDetailVO { ProductId = 2, Count = 1 }
        };

        public CartControllerCheckoutTests()
        {
            _cartRepository.Setup(r => r.FindCartByUserId(UserId)).ReturnsAsync(new CartVO
            {
                CartHeader = new CartHeaderVO { Id = 10, UserId = UserId },
                CartDetails = Items
            });
            _couponRepository.Setup(r => r.GetCouponByCouponCode("GEEK10", AccessToken))
                .ReturnsAsync(new CouponVO { CouponCode = "GEEK10", DiscountAmount = 10 });
        }

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

        [Fact]
        public async Task Checkout_PublishesTheOrderAndClearsTheCart()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId, PurchaseAmount = 150 });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var order = Assert.IsType<CheckoutHeaderVO>(ok.Value);
            Assert.Equal(Items, order.CartDetails);
            Assert.NotNull(order.DateTime);
            _messageSender.Verify(s => s.SendMessage(order, "checkoutqueue"), Times.Once);
            _cartRepository.Verify(r => r.ClearCart(UserId), Times.Once);
        }

        [Fact]
        public async Task Checkout_WithMatchingCouponDiscount_IsAccepted()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId, CouponCode = "GEEK10", DiscountTotal = 10 });

            Assert.IsType<OkObjectResult>(result.Result);
            _couponRepository.Verify(r => r.GetCouponByCouponCode("GEEK10", AccessToken), Times.Once);
            _messageSender.Verify(s => s.SendMessage(It.IsAny<BaseMessage>(), "checkoutqueue"), Times.Once);
        }

        [Fact]
        public async Task Checkout_WithDiscountThatDoesNotMatchTheCoupon_Returns412AndKeepsTheCart()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = UserId, CouponCode = "GEEK10", DiscountTotal = 50 });

            Assert.Equal(412, Assert.IsType<StatusCodeResult>(result.Result).StatusCode);
            _messageSender.Verify(s => s.SendMessage(It.IsAny<BaseMessage>(), It.IsAny<string>()), Times.Never);
            _cartRepository.Verify(r => r.ClearCart(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Checkout_WithoutUser_ReturnsBadRequest()
        {
            var result = await CreateController().Checkout(new CheckoutHeaderVO { UserId = null! });

            Assert.IsType<BadRequestResult>(result.Result);
            _messageSender.Verify(s => s.SendMessage(It.IsAny<BaseMessage>(), It.IsAny<string>()), Times.Never);
        }
    }
}
