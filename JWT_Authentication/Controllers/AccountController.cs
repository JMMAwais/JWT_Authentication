using JWT_Authentication.Data;
using JWT_Authentication.Helper;
using JWT_Authentication.Models;
using JWT_Authentication.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JWT_Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 IConfiguration configuration,
                                 TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<string> Post([FromBody] RegisterVM  model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState).ToString();
            }
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name
            };
            var result =await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var RoleExists =await _roleManager.RoleExistsAsync("User");
                if (RoleExists)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return "Registered Successfully";
                }  
            }
            return result.ToString();
        }



        [HttpPost("Login")]
        [Authorize(Policy = "AllowUser")]
        public async Task<ResponseModel> Login([FromBody] LoginVM model)
        {
            ResponseModel responseModel = new ResponseModel();

            if (ModelState.IsValid)
            {
                var loggedUser= await _userManager.FindByEmailAsync(model.Email);
                if (loggedUser == null)
                {
                   responseModel.Message= "Invalid Email Address";
                   responseModel.Status= StatusCodes.Status401Unauthorized.ToString();  
                }
                //if (_userManager.CheckPasswordAsync(loggedUser, model.Password).Result == false)
                //{
                //    return "Invalid Password";
                //}
                var result = _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true).Result;
                if (result.Succeeded)
                {
                   
                    string token = IssueToken(loggedUser).Result;
                    responseModel.Token = token;
                    responseModel.Status = Ok(token).StatusCode.ToString();
                    responseModel.Message = "User successfully loggedIn";
                    //return Ok(token).StatusCode.ToString();
                    return responseModel;
                }
                else
                {
                    if (result.IsLockedOut)
                    {
                        responseModel.Message = "The Account is LockedOut";
                        return responseModel;
                    }

                    responseModel.Status = StatusCodes.Status401Unauthorized.ToString();
                }  
            }
            return responseModel;
        }

        // revoke Token
        [HttpPost("Revoke")]
        [Authorize]
        public IActionResult RevokeToken([FromBody] string  Revoke)
        {
           // var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(Revoke))
            {
                return BadRequest("Token is required");
            }
            _tokenService.RevokeToken(Revoke);
            return Ok(new { message= "Token has been revoked"});
        }

            private async Task<string> IssueToken(AppUser user)
            {
               var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
               var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
               var userRoles = await _userManager.GetRolesAsync(user);
            // var userRole =await _roleManager.Roles.Where(x=>x.Equals(user.Id)).ToListAsync();
            var claims = new List<Claim>
               {
                    new Claim("User_Id", user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.Role,userRoles.FirstOrDefault().ToString() )
               };

               var token = new JwtSecurityToken(
               issuer: _configuration["Jwt:Issuer"],
               audience: _configuration["Jwt:Audience"],
               claims: claims,
               expires: DateTime.Now.AddHours(1),
               signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
            }



        // GET api/<AccountController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {   
            return "value";
        } 

    }
}
