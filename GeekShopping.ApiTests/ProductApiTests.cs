using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace GeekShopping.ApiTests
{
    public class ProductApiTests : IClassFixture<ProductApiFactory>
    {
        private readonly ProductApiFactory _factory;

        public ProductApiTests(ProductApiFactory factory)
        {
            _factory = factory;
        }

        private static StringContent Json(string body) => new StringContent(body, Encoding.UTF8, "application/json");

        private static object Product(string name = "Geek mug", decimal price = 69.90m, string? description = "A mug") =>
            new { name, price, description, categoryName = "Mug", imageURL = "https://example.com/mug.png" };

        private static async Task<long> CreateAsAdminAsync(HttpClient admin)
        {
            var response = await admin.PostAsJsonAsync("/api/v1/product", Product());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetInt64();
        }

        // ---------- reading ----------

        [Fact]
        public async Task ListingProducts_IsPublicAndReturnsTheSeededCatalogue()
        {
            var products = JsonDocument.Parse(await _factory.CreateClient().GetStringAsync("/api/v1/product")).RootElement;

            Assert.True(products.GetArrayLength() >= ProductApiFactory.SeededProducts);
        }

        [Fact]
        public async Task ProductDetails_WithoutToken_Returns401()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().GetAsync("/api/v1/product/1")).StatusCode);
        }

        [Fact]
        public async Task ProductDetails_UnknownId_Returns404()
        {
            var client = _factory.ClientWithRole("Client");

            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/product/999999")).StatusCode);
        }

        [Fact]
        public async Task ProductDetails_IdThatIsNotANumber_Returns400()
        {
            var client = _factory.ClientWithRole("Client");

            Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/product/abc")).StatusCode);
        }

        // ---------- permissions ----------

        [Fact]
        public async Task CreateProduct_WithoutToken_Returns401()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().PostAsJsonAsync("/api/v1/product", Product())).StatusCode);
        }

        // Kept on purpose: create and update only require a signed-in user, not an Admin.
        // If that ever changes to Admin only, this test should become a 403.
        [Fact]
        public async Task CreateProduct_AsAClient_IsAllowed()
        {
            var response = await _factory.ClientWithRole("Client").PostAsJsonAsync("/api/v1/product", Product("Client-made product"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_AsAClient_Returns403()
        {
            var id = await CreateAsAdminAsync(_factory.ClientWithRole("Admin"));

            var response = await _factory.ClientWithRole("Client").DeleteAsync($"/api/v1/product/{id}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_AsAdmin_RemovesIt()
        {
            var admin = _factory.ClientWithRole("Admin");
            var id = await CreateAsAdminAsync(admin);

            Assert.Equal(HttpStatusCode.OK, (await admin.DeleteAsync($"/api/v1/product/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/v1/product/{id}")).StatusCode);
        }

        // ---------- wrong data ----------

        [Theory]
        [InlineData(@"{ ""name"": ""Mug"", ""price"": ""cheap"" }")]     // text in a decimal
        [InlineData(@"{ ""name"": 42, ""price"": 10 }")]                // number in a string
        [InlineData(@"{ ""name"": ""Mug"", ""price"": [10] }")]          // array in a decimal
        [InlineData(@"{ ""name"": ""Mug"", ""price"": ")]               // cut off
        public async Task CreateProduct_WrongJson_Returns400(string body)
        {
            var response = await _factory.ClientWithRole("Admin").PostAsync("/api/v1/product", Json(body));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(0.99)]
        public async Task CreateProduct_PriceBelowOne_Returns400(decimal price)
        {
            var response = await _factory.ClientWithRole("Admin").PostAsJsonAsync("/api/v1/product", Product(price: price));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Price must be between 1 and 999999999999.", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateProduct_WithoutName_Returns400(string? name)
        {
            var response = await _factory.ClientWithRole("Admin").PostAsJsonAsync("/api/v1/product", Product(name: name!));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_TextsLongerThanTheColumns_Return400InsteadOfASqlError()
        {
            var admin = _factory.ClientWithRole("Admin");

            var longName = await admin.PostAsJsonAsync("/api/v1/product", Product(name: new string('n', 151)));
            var longDescription = await admin.PostAsJsonAsync("/api/v1/product", Product(description: new string('d', 501)));

            Assert.Equal(HttpStatusCode.BadRequest, longName.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, longDescription.StatusCode);
            Assert.Contains("Description should have at most 500 characters.", await longDescription.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CreateProduct_ValuesAtTheLimits_AreAccepted()
        {
            var response = await _factory.ClientWithRole("Admin").PostAsJsonAsync("/api/v1/product",
                Product(name: new string('n', 150), price: 1m, description: new string('d', 500)));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidPrice_Returns400AndKeepsThePrice()
        {
            var admin = _factory.ClientWithRole("Admin");
            var id = await CreateAsAdminAsync(admin);

            var response = await admin.PutAsJsonAsync("/api/v1/product", new { id, name = "Geek mug", price = -1 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var product = JsonDocument.Parse(await admin.GetStringAsync($"/api/v1/product/{id}")).RootElement;
            Assert.Equal(69.90m, product.GetProperty("price").GetDecimal());
        }
    }
}
