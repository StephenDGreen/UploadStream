using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Upload.Models;

namespace Upload.API
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public SignInManager<IdentityUser> _signInManager { get; }
        public UserManager<IdentityUser> _userManager { get; }
        public IConfiguration _configuration { get; }

        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (result.Succeeded)
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        if (userRoles != null)
                        {
                            var claims = new[]
                            {
                                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                new Claim(ClaimTypes.Role, userRoles.Contains("Admin") ? "Admin" : "Customer")
                            };
                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                            var token = new JwtSecurityToken(_configuration["Tokens:Issuer"],
                              _configuration["Tokens:Issuer"],
                              claims,
                              expires: DateTime.Now.AddMinutes(30),
                              signingCredentials: creds);

                            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                        }
                    }
                }
            }

            return BadRequest("Could not create token");
        }
    }
}