using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Email.Model.Context
{
    public class SqlServerContext : DbContext
    {
        public SqlServerContext()
        {

        }
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }

        public DbSet<EmailLog> EmailLogs { get; set; }
    }
}