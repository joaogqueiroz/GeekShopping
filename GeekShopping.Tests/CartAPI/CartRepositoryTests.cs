using AutoMapper;
using GeekShopping.CartAPI.Config;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model.Context;
using GeekShopping.CartAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Tests.CartAPI
{
    [Collection(SqlServerCollection.Name)]
    public class CartRepositoryTests
    {
        private readonly DbContextOptions<SqlServerContext> _options;
        private readonly IMapper _mapper = MappingConfig.RegisterMaps().CreateMapper();

        public CartRepositoryTests(SqlServerFixture fixture)
        {
            _options = fixture.CreateDatabase<SqlServerContext>(options => new SqlServerContext(options));
        }

        // A fresh context per call, like one per HTTP request in the API.
        private CartRepository Repository() => new CartRepository(new SqlServerContext(_options), _mapper);

        private static string NewUser() => Guid.NewGuid().ToString();

        private static CartVO AddItem(string userId, long productId, int count) => new CartVO
        {
            CartHeader = new CartHeaderVO { UserId = userId, CouponCode = "" },
            CartDetails = new[]
            {
                new CartDetailVO
                {
                    ProductId = productId,
                    Count = count,
                    Product = new ProductVO
                    {
                        Id = productId,
                        Name = $"Product {productId}",
                        Price = 49.90m,
                        Description = "A product",
                        CategoryName = "T-shirt",
                        ImageURL = "https://example.com/image.png"
                    }
                }
            }
        };

        [Fact]
        public async Task SaveOrUpdateCart_NewUser_CreatesCartWithTheItem()
        {
            var userId = NewUser();

            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1001, count: 2));

            var cart = await Repository().FindCartByUserId(userId);
            Assert.Equal(userId, cart.CartHeader!.UserId);
            var item = Assert.Single(cart.CartDetails!);
            Assert.Equal(1001, item.ProductId);
            Assert.Equal(2, item.Count);
            Assert.Equal("Product 1001", item.Product!.Name);
        }

        [Fact]
        public async Task SaveOrUpdateCart_SameProductAgain_AddsToTheCount()
        {
            var userId = NewUser();

            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1002, count: 2));
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1002, count: 3));

            var item = Assert.Single((await Repository().FindCartByUserId(userId)).CartDetails!);
            Assert.Equal(5, item.Count);
        }

        [Fact]
        public async Task SaveOrUpdateCart_DifferentProducts_KeepsOneCartWithBothItems()
        {
            var userId = NewUser();

            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1003, count: 1));
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1004, count: 1));

            var cart = await Repository().FindCartByUserId(userId);
            Assert.Equal(new long?[] { 1003, 1004 }, cart.CartDetails!.Select(d => d.ProductId).OrderBy(id => id));
            Assert.Equal(cart.CartHeader!.Id, cart.CartDetails!.Select(d => d.CartHeaderId).Distinct().Single());
        }

        [Fact]
        public async Task ApplyAndRemoveCoupon_UpdateTheCartHeader()
        {
            var userId = NewUser();
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1005, count: 1));

            Assert.True(await Repository().ApplyCoupon(userId, "GEEK10"));
            Assert.Equal("GEEK10", (await Repository().FindCartByUserId(userId)).CartHeader!.CouponCode);

            Assert.True(await Repository().RemoveCoupon(userId));
            Assert.Equal("", (await Repository().FindCartByUserId(userId)).CartHeader!.CouponCode);
        }

        [Fact]
        public async Task ApplyCoupon_UserWithoutCart_ReturnsFalse()
        {
            Assert.False(await Repository().ApplyCoupon(NewUser(), "GEEK10"));
        }

        [Fact]
        public async Task RemoveFromCart_OneOfTwoItems_KeepsTheCart()
        {
            var userId = NewUser();
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1006, count: 1));
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1007, count: 1));
            var toRemove = (await Repository().FindCartByUserId(userId)).CartDetails!.Single(d => d.ProductId == 1006);

            Assert.True(await Repository().RemoveFromCart(toRemove.Id));

            var cart = await Repository().FindCartByUserId(userId);
            Assert.NotEqual(0, cart.CartHeader!.Id);
            Assert.Equal(1007, Assert.Single(cart.CartDetails!).ProductId);
        }

        [Fact]
        public async Task RemoveFromCart_LastItem_RemovesTheCart()
        {
            var userId = NewUser();
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1008, count: 1));
            var item = Assert.Single((await Repository().FindCartByUserId(userId)).CartDetails!);

            Assert.True(await Repository().RemoveFromCart(item.Id));

            var cart = await Repository().FindCartByUserId(userId);
            Assert.Equal(0, cart.CartHeader!.Id);
            Assert.Empty(cart.CartDetails!);
        }

        [Fact]
        public async Task ClearCart_RemovesHeaderAndItems()
        {
            var userId = NewUser();
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1009, count: 1));
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1010, count: 4));

            Assert.True(await Repository().ClearCart(userId));

            var cart = await Repository().FindCartByUserId(userId);
            Assert.Equal(0, cart.CartHeader!.Id);
            Assert.Empty(cart.CartDetails!);
        }

        [Fact]
        public async Task ClearCart_UserWithoutCart_ReturnsFalse()
        {
            Assert.False(await Repository().ClearCart(NewUser()));
        }

        [Fact]
        public async Task RemoveFromCart_UnknownItem_ReturnsFalseAndKeepsOtherCarts()
        {
            var userId = NewUser();
            await Repository().SaveOrUpdateCart(AddItem(userId, productId: 1011, count: 1));

            Assert.False(await Repository().RemoveFromCart(999_999));

            Assert.Single((await Repository().FindCartByUserId(userId)).CartDetails!);
        }

        [Fact]
        public async Task RemoveCoupon_UserWithoutCart_ReturnsFalse()
        {
            Assert.False(await Repository().RemoveCoupon(NewUser()));
        }

        [Fact]
        public async Task FindCartByUserId_UserWithoutCart_ReturnsAnEmptyCartNotNull()
        {
            // Checkout relies on this: it must look at the items, not at a null cart
            var cart = await Repository().FindCartByUserId(NewUser());

            Assert.NotNull(cart);
            Assert.Equal(0, cart.CartHeader!.Id);
            Assert.Empty(cart.CartDetails!);
        }
    }
}
