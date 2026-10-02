using AutoMapper;
using GeekShopping.ProductAPI.Config;
using GeekShopping.ProductAPI.Data.ValeuObjects;
using GeekShopping.ProductAPI.Model.Context;
using GeekShopping.ProductAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Tests.ProductAPI
{
    [Collection(SqlServerCollection.Name)]
    public class ProductRepositoryTests
    {
        // Products inserted by the ProductAPI migrations (HasData in SqlServerContext).
        private const int SeededProducts = 12;

        private readonly DbContextOptions<SqlServerContext> _options;
        private readonly IMapper _mapper = MappingConfig.RegisterMaps().CreateMapper();

        public ProductRepositoryTests(SqlServerFixture fixture)
        {
            _options = fixture.CreateDatabase<SqlServerContext>(options => new SqlServerContext(options));
        }

        // A fresh context per call, like one per HTTP request in the API.
        private ProductRepository Repository() => new ProductRepository(new SqlServerContext(_options), _mapper);

        private static ProductVO NewProduct(string name) => new ProductVO
        {
            Name = name,
            Price = 69.90m,
            Description = "A new product",
            CategoryName = "T-shirt",
            ImageURL = "https://example.com/image.png"
        };

        [Fact]
        public async Task FindAll_ReturnsTheSeededCatalogue()
        {
            var products = await Repository().FindAll();

            Assert.Equal(SeededProducts, products.Count());
            Assert.All(products, p => Assert.False(string.IsNullOrWhiteSpace(p.Name)));
        }

        [Fact]
        public async Task Create_AssignsAnIdAndCanBeFound()
        {
            var created = await Repository().Create(NewProduct("Geek mug"));

            Assert.True(created.Id > 0);
            var found = await Repository().FindById(created.Id);
            Assert.Equal("Geek mug", found.Name);
            Assert.Equal(69.90m, found.Price);
            Assert.Equal(SeededProducts + 1, (await Repository().FindAll()).Count());
        }

        [Fact]
        public async Task Update_ChangesTheProduct()
        {
            var created = await Repository().Create(NewProduct("Draft name"));

            created.Name = "Final name";
            created.Price = 99.90m;
            await Repository().Update(created);

            var updated = await Repository().FindById(created.Id);
            Assert.Equal("Final name", updated.Name);
            Assert.Equal(99.90m, updated.Price);
        }

        [Fact]
        public async Task Delete_RemovesTheProduct()
        {
            var created = await Repository().Create(NewProduct("To delete"));

            Assert.True(await Repository().Delete(created.Id));

            Assert.Null(await Repository().FindById(created.Id));
        }

        [Fact]
        public async Task Delete_UnknownProduct_ReturnsFalse()
        {
            Assert.False(await Repository().Delete(999_999));
        }

        [Fact]
        public async Task FindById_UnknownProduct_ReturnsNull()
        {
            Assert.Null(await Repository().FindById(999_999));
        }
    }
}
