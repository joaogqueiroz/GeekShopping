using System.ComponentModel.DataAnnotations;
using GeekShopping.CartAPI.Controllers;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GeekShopping.Tests.CartAPI
{
    public class CartInputValidationTests
    {
        private readonly Mock<ICartRepository> _cartRepository = new();

        // Signed in as user-1, the owner of the carts built below
        private CartController CreateController() =>
            new CartController(_cartRepository.Object, new Mock<IRabbitMQMessageSender>().Object, new Mock<ICouponRepository>().Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "user-1") }, "Bearer"))
                    }
                }
            };

        private static CartVO Cart(params CartDetailVO[] items) => new CartVO
        {
            CartHeader = new CartHeaderVO { UserId = "user-1" },
            CartDetails = items
        };

        // [ApiController] turns these attribute errors into a 400 before the action runs.
        private static List<ValidationResult> Validate(CartDetailVO item)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(item, new ValidationContext(item), results, validateAllProperties: true);
            return results;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(int.MinValue)]
        public void ItemCountBelowOne_IsInvalid(int count)
        {
            var errors = Validate(new CartDetailVO { ProductId = 1, Count = count });

            Assert.Contains(errors, e => e.ErrorMessage == "Count must be at least 1.");
        }

        [Fact]
        public void ItemWithoutCount_IsInvalid()
        {
            var errors = Validate(new CartDetailVO { ProductId = 1, Count = null });

            Assert.Contains(errors, e => e.ErrorMessage == "Count is required.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void ItemCountOfOneOrMore_IsValid(int count)
        {
            Assert.Empty(Validate(new CartDetailVO { ProductId = 1, Count = count }));
        }

        [Fact]
        public async Task AddCart_WithoutItems_Returns400AndSavesNothing()
        {
            var result = await CreateController().AddCart(Cart());

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _cartRepository.Verify(r => r.SaveOrUpdateCart(It.IsAny<CartVO>()), Times.Never);
        }

        [Fact]
        public async Task AddCart_WithNullItemList_Returns400()
        {
            var result = await CreateController().AddCart(new CartVO { CartHeader = new CartHeaderVO { UserId = "user-1" }, CartDetails = null });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddCart_WithoutHeader_Returns400()
        {
            var result = await CreateController().AddCart(new CartVO { CartDetails = new[] { new CartDetailVO { ProductId = 1, Count = 1 } } });

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _cartRepository.Verify(r => r.SaveOrUpdateCart(It.IsAny<CartVO>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCart_WithoutItems_Returns400()
        {
            var result = await CreateController().UpdateCart(Cart());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddCart_ValidCart_IsSaved()
        {
            var cart = Cart(new CartDetailVO { ProductId = 1, Count = 2 });
            _cartRepository.Setup(r => r.SaveOrUpdateCart(cart)).ReturnsAsync(cart);

            var result = await CreateController().AddCart(cart);

            Assert.IsType<OkObjectResult>(result.Result);
            _cartRepository.Verify(r => r.SaveOrUpdateCart(cart), Times.Once);
        }

        [Fact]
        public async Task RemoveCart_ItemNotInTheUsersCart_Returns404AndRemovesNothing()
        {
            _cartRepository.Setup(r => r.FindCartByUserId("user-1"))
                .ReturnsAsync(Cart(new CartDetailVO { Id = 1, ProductId = 1, Count = 1 }));

            // 999 is not in user-1's cart: unknown, or someone else's
            var result = await CreateController().RemoveCart(999);

            Assert.IsType<NotFoundResult>(result.Result);
            _cartRepository.Verify(r => r.RemoveFromCart(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task RemoveCart_ItemInTheUsersCart_IsRemoved()
        {
            _cartRepository.Setup(r => r.FindCartByUserId("user-1"))
                .ReturnsAsync(Cart(new CartDetailVO { Id = 1, ProductId = 1, Count = 1 }));
            _cartRepository.Setup(r => r.RemoveFromCart(1)).ReturnsAsync(true);

            var result = await CreateController().RemoveCart(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddCart_ForAnotherUser_IsForbidden()
        {
            var cart = new CartVO
            {
                CartHeader = new CartHeaderVO { UserId = "another-user" },
                CartDetails = new[] { new CartDetailVO { ProductId = 1, Count = 1 } }
            };

            var result = await CreateController().AddCart(cart);

            Assert.IsType<ForbidResult>(result.Result);
            _cartRepository.Verify(r => r.SaveOrUpdateCart(It.IsAny<CartVO>()), Times.Never);
        }
    }
}
