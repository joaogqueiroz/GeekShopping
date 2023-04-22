using Microsoft.EntityFrameworkCore;

namespace GeekShoppping.ProductAPI.Model.Context
{
    public class SqlServerContext : DbContext
    {
        public SqlServerContext()
        {
            
        }
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options){}

        public DbSet<Product> Products {get; set;}
    }
}