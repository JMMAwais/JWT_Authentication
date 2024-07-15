using JWT_Authentication.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT_Authentication.Helper
{
    public  class AuthHelper
    {
        private readonly IConfiguration _configuration;
        public AuthHelper(IConfiguration configuration)
        {
            _configuration= configuration;
        }
        public static string GenerateJWTToken(AppUser user)
        {
            var claims = new List<Claim> {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
    };
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes("ApplicationSettings:JWT_Secret")
                        ),
                    SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public static string GenerateJWTToken()
        {

            return "";
        }
    }
}
