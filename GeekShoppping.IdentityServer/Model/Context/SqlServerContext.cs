using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeekShoppping.IdentityServer.Model.Context
{
    public class SqlServerContext : IdentityDbContext<ApplicationUser>
    {

        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }

    }
}