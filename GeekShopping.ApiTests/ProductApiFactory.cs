using GeekShopping.ProductAPI.Controllers;
using GeekShopping.ProductAPI.Model.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace GeekShopping.ApiTests
{
    // Runs the real ProductAPI against SQL Server in a container, with the migrations (and their
    // 12 seeded products) applied first.
    public class ProductApiFactory : WebApplicationFactory<ProductController>, IAsyncLifetime
    {
        public const int SeededProducts = 12;

        private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public async Task InitializeAsync()
        {
            await _sqlServer.StartAsync();
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<SqlServerContext>().Database.Migrate();
        }

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await _sqlServer.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:GeekShoppingProductCs", new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
            {
                InitialCatalog = "geek_shopping_product_api_tests"
            }.ConnectionString);

            builder.ConfigureTestServices(services => services.UseTestTokens());
        }

        public HttpClient ClientWithRole(string role) =>
            CreateClient().WithToken(TestTokens.Create(Guid.NewGuid().ToString(), role: role));
    }
}
