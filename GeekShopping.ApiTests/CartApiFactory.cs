using System.Collections.Concurrent;
using GeekShopping.CartAPI.Controllers;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model.Context;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using GeekShopping.MessageBus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace GeekShopping.ApiTests
{
    // Runs the real CartAPI against SQL Server in a container. The coupon API and RabbitMQ are
    // replaced by fakes; the fake sender keeps every published message so tests can inspect it.
    // WebApplicationFactory<CartController>: any type from the CartAPI assembly picks its Program.
    public class CartApiFactory : WebApplicationFactory<CartController>, IAsyncLifetime
    {
        public const string KnownCoupon = "GEEK10";

        private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public FakeMessageSender Messages { get; } = new FakeMessageSender();

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
            builder.UseSetting("ConnectionStrings:GeekShoppingCartCs", new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
            {
                InitialCatalog = "geek_shopping_cart_api_tests"
            }.ConnectionString);

            builder.ConfigureTestServices(services =>
            {
                services.UseTestTokens();
                services.RemoveAll<ICouponRepository>();
                services.AddScoped<ICouponRepository, FakeCouponRepository>();
                services.RemoveAll<IRabbitMQMessageSender>();
                services.AddSingleton<IRabbitMQMessageSender>(Messages);
            });
        }

        public HttpClient ClientFor(string userId, string? scope = TestTokens.Scope) =>
            CreateClient().WithToken(TestTokens.Create(userId, scope));

        private class FakeCouponRepository : ICouponRepository
        {
            // The real repository answers an unknown code with an empty coupon
            public Task<CouponVO> GetCouponByCouponCode(string couponCode, string token) =>
                Task.FromResult(couponCode == KnownCoupon
                    ? new CouponVO { CouponCode = KnownCoupon, DiscountAmount = 10 }
                    : new CouponVO());
        }

        public class FakeMessageSender : IRabbitMQMessageSender
        {
            public ConcurrentQueue<(BaseMessage Message, string Queue)> Sent { get; } = new();

            public void SendMessage(BaseMessage baseMessage, string queueName) => Sent.Enqueue((baseMessage, queueName));
        }
    }
}
