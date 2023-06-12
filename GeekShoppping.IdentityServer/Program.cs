using GeekShoppping.IdentityServer.Configuration;
using GeekShoppping.IdentityServer.Initializer;
using GeekShoppping.IdentityServer.Model;
using GeekShoppping.IdentityServer.Model.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("GeekShoppingIdentityServerCs");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<SqlServerContext>()
                .AddDefaultTokenProviders();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

var builderServices = builder.Services.AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.EmitStaticAudienceClaim = true;
    })
        .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
        .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
        .AddInMemoryClients(IdentityConfiguration.Clients)
        .AddAspNetIdentity<ApplicationUser>();

builderServices.AddDeveloperSigningCredential();

builder.Services.AddDbContext<SqlServerContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllersWithViews();

var app = builder.Build();

var initialize = app.Services.CreateScope().ServiceProvider.GetService<IDbInitializer>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

initialize.Initialize();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
