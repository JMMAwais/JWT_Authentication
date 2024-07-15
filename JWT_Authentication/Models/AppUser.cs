using Microsoft.AspNetCore.Identity;

namespace JWT_Authentication.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
    }
}
