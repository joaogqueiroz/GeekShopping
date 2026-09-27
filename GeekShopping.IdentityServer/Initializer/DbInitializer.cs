using System.Security.Claims;
using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly SqlServerContext _dbContext;
        private readonly UserManager<ApplicationUser> _user;
        private readonly RoleManager<IdentityRole> _role;
    public DbInitializer(SqlServerContext dbContext, 
                        UserManager<ApplicationUser> user, 
                        RoleManager<IdentityRole> role)
    {
        _dbContext = dbContext;
        _user = user;
        _role = role;
    }

        public void Initialize()
        {
            if(_role.FindByNameAsync(IdentityConfiguration.Admin).Result != null) return;
            _role.CreateAsync(new IdentityRole(IdentityConfiguration.Admin)).GetAwaiter().GetResult();
            _role.CreateAsync(new IdentityRole(IdentityConfiguration.Client)).GetAwaiter().GetResult();

            ApplicationUser admin = new ApplicationUser()
            {
                UserName = "Admin",
                Email = "admin@admin.com",
                EmailConfirmed = true,
                PhoneNumber = "+5521999999999",
                FirstName = "Admin",
                LastName = "Test",
            };
            _user.CreateAsync(admin, "Test123@").GetAwaiter().GetResult();
            _user.AddToRoleAsync(admin, IdentityConfiguration.Admin).GetAwaiter().GetResult();
            var adminClaims = _user.AddClaimsAsync(admin, new Claim[] 
            {
                new Claim(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                new Claim(JwtClaimTypes.GivenName, admin.FirstName),
                new Claim(JwtClaimTypes.FamilyName, admin.LastName),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Admin)                
            }).Result;

            ApplicationUser client = new ApplicationUser()
            {
                UserName = "client",
                Email = "client@client.com",
                EmailConfirmed = true,
                PhoneNumber = "+5521999999999",
                FirstName = "Client",
                LastName = "Test",
            };
            _user.CreateAsync(client, "Test123@").GetAwaiter().GetResult();
            _user.AddToRoleAsync(client, IdentityConfiguration.Client).GetAwaiter().GetResult();
            var clientClaims = _user.AddClaimsAsync(client, new Claim[] 
            {
                new Claim(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                new Claim(JwtClaimTypes.GivenName, client.FirstName),
                new Claim(JwtClaimTypes.FamilyName, client.LastName),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.Client)                
            }).Result;
        } 
    }
}