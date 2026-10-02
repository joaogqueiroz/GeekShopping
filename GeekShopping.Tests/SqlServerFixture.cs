using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace GeekShopping.Tests
{
    // One SQL Server container for the whole run. Each test class gets its own database,
    // built with that service's real EF Core migrations.
    public class SqlServerFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public async Task InitializeAsync() => await _container.StartAsync();

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        public DbContextOptions<TContext> CreateDatabase<TContext>(Func<DbContextOptions<TContext>, TContext> createContext)
            where TContext : DbContext
        {
            var connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                InitialCatalog = $"geek_shopping_test_{Guid.NewGuid():N}"
            }.ConnectionString;

            var options = new DbContextOptionsBuilder<TContext>().UseSqlServer(connectionString).Options;
            using var context = createContext(options);
            context.Database.Migrate();
            return options;
        }
    }

    [CollectionDefinition(Name)]
    public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
    {
        public const string Name = "SqlServer";
    }
}
