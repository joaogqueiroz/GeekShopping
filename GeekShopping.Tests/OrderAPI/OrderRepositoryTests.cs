using GeekShopping.OrderAPI.MessageConsumer;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model.Context;
using GeekShopping.OrderAPI.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GeekShopping.Tests.OrderAPI
{
    [Collection(SqlServerCollection.Name)]
    public class OrderRepositoryTests
    {
        private readonly DbContextOptions<SqlServerContext> _options;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests(SqlServerFixture fixture)
        {
            _options = fixture.CreateDatabase<SqlServerContext>(options => new SqlServerContext(options));
            _repository = new OrderRepository(_options);
        }

        private static CheckoutHeaderVO Checkout() => new CheckoutHeaderVO
        {
            UserId = "user-1",
            PurchaseAmount = 130m,
            DiscountTotal = 0m,
            FirstName = "Test",
            LastName = "User",
            CardNumber = "4111111111111111",
            CVV = "123",
            ExpiryMonthYear = "12/30",
            CartDetails = new[]
            {
                new CartDetailVO { ProductId = 1, Count = 2, Product = new ProductVO { Id = 1, Name = "T-shirt", Price = 50m } },
                new CartDetailVO { ProductId = 2, Count = 1, Product = new ProductVO { Id = 2, Name = "Mug", Price = 30m } }
            }
        };

        [Fact]
        public async Task AddOrder_SavesTheOrderItsItemsAndTheItemCount()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout());

            Assert.True(await _repository.AddOrder(order));

            await using var db = new SqlServerContext(_options);
            var saved = await db.Headers.Include(h => h.OrderDetails).SingleAsync(h => h.Id == order.Id);
            Assert.Equal(3, saved.OrderTotal);
            Assert.Equal(130m, saved.PurchaseAmount);
            Assert.False(saved.PaymentStatus);
            Assert.Equal(new[] { "Mug", "T-shirt" }, saved.OrderDetails!.Select(d => d.ProductName).OrderBy(n => n));
        }

        [Fact]
        public async Task AddOrder_StoresOnlyTheLastFourDigitsAndNoCvv()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout());
            await _repository.AddOrder(order);

            await using var db = new SqlServerContext(_options);
            var saved = await db.Headers.SingleAsync(h => h.Id == order.Id);
            Assert.Equal("**** **** **** 1111", saved.CardNumber);
            Assert.Null(saved.CVV);
        }

        // Orders saved before the change held the whole card and the CVV
        [Fact]
        public async Task Migration_MasksTheCardsOfOrdersSavedBefore()
        {
            await using var db = new SqlServerContext(_options);
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync("20230803010247_AddOrderDataTablesOnDB");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO order_header (card_number, cvv, expiry_month_year, payment_status, order_total_itens, purchase_amount, discount_total, purchase_date, order_time) " +
                "VALUES ('4111111111111111', '123', '12/30', 0, 1, 10, 0, SYSDATETIME(), SYSDATETIME()), (NULL, NULL, NULL, 0, 1, 10, 0, SYSDATETIME(), SYSDATETIME())");

            await migrator.MigrateAsync();

            var rows = await db.Headers.AsNoTracking().OrderBy(h => h.Id).ToListAsync();
            Assert.Equal("**** **** **** 1111", rows[0].CardNumber);
            Assert.Null(rows[0].CVV);
            Assert.Equal("12/30", rows[0].ExpiryMonthYear);
            Assert.Null(rows[1].CardNumber);
        }

        [Fact]
        public async Task AddOrder_Null_ReturnsFalse()
        {
            Assert.False(await _repository.AddOrder(null!));
        }

        [Fact]
        public async Task UpdatePaymentStatus_MarksTheOrderAsPaid()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout());
            await _repository.AddOrder(order);

            await _repository.UpdateOrderPaymentStatus(order.Id, true);

            await using var db = new SqlServerContext(_options);
            Assert.True((await db.Headers.SingleAsync(h => h.Id == order.Id)).PaymentStatus);
        }

        [Fact]
        public async Task UpdatePaymentStatus_UnknownOrder_ChangesNothing()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout());
            await _repository.AddOrder(order);

            await _repository.UpdateOrderPaymentStatus(999_999, true);

            await using var db = new SqlServerContext(_options);
            Assert.False((await db.Headers.SingleAsync(h => h.Id == order.Id)).PaymentStatus);
        }
    }
}
