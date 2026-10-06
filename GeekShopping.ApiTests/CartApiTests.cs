using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GeekShopping.CartAPI.Messages;

namespace GeekShopping.ApiTests
{
    public class CartApiTests : IClassFixture<CartApiFactory>
    {
        private readonly CartApiFactory _factory;

        public CartApiTests(CartApiFactory factory)
        {
            _factory = factory;
        }

        private static string NewUser() => Guid.NewGuid().ToString();

        private static StringContent Json(string body) => new StringContent(body, Encoding.UTF8, "application/json");

        private static object AddItem(string userId, long productId, int count, decimal price) => new
        {
            cartHeader = new { userId, couponCode = "" },
            cartDetails = new[]
            {
                new { productId, count, product = new { id = productId, name = $"Product {productId}", price, categoryName = "T-shirt" } }
            }
        };

        private static async Task<JsonElement> CartOf(HttpClient client, string userId) =>
            JsonDocument.Parse(await client.GetStringAsync($"/api/v1/cart/find-cart/{userId}")).RootElement;

        // ---------- authentication and ownership ----------

        [Fact]
        public async Task WithoutToken_Returns401()
        {
            var response = await _factory.CreateClient().GetAsync($"/api/v1/cart/find-cart/{NewUser()}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TokenWithoutTheApiScope_Returns403()
        {
            var userId = NewUser();

            var response = await _factory.ClientFor(userId, scope: null).GetAsync($"/api/v1/cart/find-cart/{userId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SomeoneElsesCart_IsForbiddenForEveryAction()
        {
            var owner = NewUser();
            var ownerClient = _factory.ClientFor(owner);
            await ownerClient.PostAsJsonAsync("/api/v1/cart/add-cart", AddItem(owner, 7001, 1, 50m));
            var intruder = _factory.ClientFor(NewUser());

            Assert.Equal(HttpStatusCode.Forbidden, (await intruder.GetAsync($"/api/v1/cart/find-cart/{owner}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await intruder.PostAsJsonAsync("/api/v1/cart/add-cart", AddItem(owner, 7002, 1, 50m))).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await intruder.PutAsJsonAsync("/api/v1/cart/update-cart", AddItem(owner, 7001, 5, 50m))).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await intruder.PostAsJsonAsync("/api/v1/cart/apply-coupon", new { cartHeader = new { userId = owner, couponCode = CartApiFactory.KnownCoupon } })).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await intruder.DeleteAsync($"/api/v1/cart/remove-coupon/{owner}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await intruder.PostAsJsonAsync("/api/v1/cart/checkout", new { userId = owner })).StatusCode);

            // The owner's cart is untouched
            var item = Assert.Single((await CartOf(ownerClient, owner)).GetProperty("cartDetails").EnumerateArray());
            Assert.Equal(1, item.GetProperty("count").GetInt32());
        }

        [Fact]
        public async Task RemovingAnItemFromSomeoneElsesCart_Returns404AndKeepsIt()
        {
            var owner = NewUser();
            var ownerClient = _factory.ClientFor(owner);
            await ownerClient.PostAsJsonAsync("/api/v1/cart/add-cart", AddItem(owner, 7003, 1, 50m));
            var itemId = (await CartOf(ownerClient, owner)).GetProperty("cartDetails")[0].GetProperty("id").GetInt64();

            var response = await _factory.ClientFor(NewUser()).DeleteAsync($"/api/v1/cart/remove-cart/{itemId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Single((await CartOf(ownerClient, owner)).GetProperty("cartDetails").EnumerateArray());
        }

        // ---------- wrong data ----------

        [Theory]
        [InlineData(@"{ ""cartHeader"": { ""userId"": ""{user}"" }, ""cartDetails"": [ { ""productId"": 1, ""count"": ""two"" } ] }")]  // text in a number
        [InlineData(@"{ ""cartHeader"": { ""userId"": ""{user}"" }, ""cartDetails"": [ { ""productId"": ""abc"", ""count"": 1 } ] }")]  // text in an id
        [InlineData(@"{ ""cartHeader"": { ""userId"": ""{user}"" }, ""cartDetails"": [ { ""productId"": 1, ""count"": 1.5 } ] }")]     // decimal in an integer
        [InlineData(@"{ ""cartHeader"": { ""userId"": ""{user}"" }, ""cartDetails"": ")]                                           // cut off
        public async Task AddCart_WrongJson_Returns400(string body)
        {
            var userId = NewUser();

            var response = await _factory.ClientFor(userId).PostAsync("/api/v1/cart/add-cart", Json(body.Replace("{user}", userId)));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public async Task AddCart_QuantityBelowOne_Returns400(int count)
        {
            var userId = NewUser();

            var response = await _factory.ClientFor(userId).PostAsJsonAsync("/api/v1/cart/add-cart", AddItem(userId, 7004, count, 50m));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Count must be at least 1.", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Checkout_EmptyCart_Returns400()
        {
            var userId = NewUser();

            var response = await _factory.ClientFor(userId).PostAsJsonAsync("/api/v1/cart/checkout", new { userId });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- the whole purchase through HTTP ----------

        [Fact]
        public async Task Checkout_ChargesTheServerTotalPublishesTheOrderAndClearsTheCart()
        {
            var userId = NewUser();
            var client = _factory.ClientFor(userId);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/cart/add-cart", AddItem(userId, 7005, 2, 50.00m))).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/cart/add-cart", AddItem(userId, 7006, 1, 30.00m))).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/cart/apply-coupon", new { cartHeader = new { userId, couponCode = CartApiFactory.KnownCoupon } })).StatusCode);

            // The client claims to owe 1; the cart is worth 50 × 2 + 30 = 130, minus the 10 coupon
            var response = await client.PostAsJsonAsync("/api/v1/cart/checkout", new
            {
                userId,
                couponCode = CartApiFactory.KnownCoupon,
                discountTotal = 10,
                purchaseAmount = 1,
                firstName = "Test",
                lastName = "User",
                cardNumber = "4111111111111111",
                cvv = "123",
                expiryMonthYear = "12/30"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var published = _factory.Messages.Sent.Select(s => s).Where(s => s.Message is CheckoutHeaderVO o && o.UserId == userId).ToList();
            var (message, queue) = Assert.Single(published);
            var order = (CheckoutHeaderVO)message;
            Assert.Equal("checkoutqueue", queue);
            Assert.Equal(120.00m, order.PurchaseAmount);
            Assert.Equal(10m, order.DiscountTotal);
            Assert.Equal(2, order.CartDetails!.Count());
            Assert.Empty((await CartOf(client, userId)).GetProperty("cartDetails").EnumerateArray());
        }
    }
}
