using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GeekShopping.ApiTests
{
    // Stands in for IdentityServer: the APIs keep their real JwtBearer validation and policies,
    // but accept tokens signed with this key instead of fetching IdentityServer's signing keys.
    public static class TestTokens
    {
        private static readonly SymmetricSecurityKey Key =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("geek-shopping-api-tests-signing-key-0123456789"));

        public const string Scope = "geek_shopping";

        public static string Create(string userId, string? scope = Scope, string? role = null)
        {
            var claims = new List<Claim> { new Claim("sub", userId) };
            if (scope != null) claims.Add(new Claim("scope", scope));
            if (role != null) claims.Add(new Claim("role", role));

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static HttpClient WithToken(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // Replaces IdentityServer as the token source for the "Bearer" scheme each API registers.
        public static void UseTestTokens(this IServiceCollection services) =>
            services.PostConfigure<JwtBearerOptions>("Bearer", options =>
            {
                options.Authority = null;
                options.MetadataAddress = null!;
                options.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = Key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RoleClaimType = "role"
                };
            });
    }
}
