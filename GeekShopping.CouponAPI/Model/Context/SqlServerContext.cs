using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CouponAPI.Model.Context
{
    public class SqlServerContext : DbContext
    {
        public SqlServerContext()
        {

        }
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }

        public DbSet<Coupon> Coupons { get; set; }
        }
}