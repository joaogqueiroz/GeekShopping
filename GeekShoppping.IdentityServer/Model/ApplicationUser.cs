using Microsoft.AspNetCore.Identity;

namespace GeekShoppping.IdentityServer.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FristName { get; set; }
        public string LastName { get; set; }

    }
}
