using System.ComponentModel.DataAnnotations;
using GeekShopping.CartAPI.Controllers;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GeekShopping.Tests.CartAPI
{
    public class CartInputValidationTests
    {
        private readonly Mock<ICartRepository> _cartRepository = new();

        private CartController CreateController() =>
            new CartController(_cartRepository.Object, new Mock<IRabbitMQMessageSender>().Object, new Mock<ICouponRepository>().Object);

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
        public async Task RemoveCart_UnknownItem_Returns400()
        {
            _cartRepository.Setup(r => r.RemoveFromCart(999)).ReturnsAsync(false);

            var result = await CreateController().RemoveCart(999);

            Assert.IsType<BadRequestResult>(result.Result);
        }
    }
}
