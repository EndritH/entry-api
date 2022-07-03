
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Auth;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController( IConfiguration configuration, UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        private async Task<User> GetUser(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        // POST: api/Users
        // register user 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("/api/register/guest")]
        public async Task<ActionResult<User>> RegisterUser(Register model)
        {
            if ((await _userManager.FindByEmailAsync(model.Username))!=null)
            {
                return Problem(" User already exists ");
            }
            User user = new()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
            };

            var result = await _userManager.CreateAsync(user,model.Password);
            if (!result.Succeeded)
            {
                return this.Problem("User could not be added");
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Guest))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Guest);
            }

            return Ok(user);
        }

        // POST: api/Users
        // register user 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("/api/register/employee")]
        public async Task<ActionResult<User>> RegisterEmployee(Register model)
        {
            if ((await _userManager.FindByEmailAsync(model.Username)) != null)
            {
                return Problem(" User already exists ");
            }
            User user = new()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return this.Problem("User could not be added");
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Employee))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Employee);
            }
            return Ok(user);
        }

        // POST: api/Users
        // register user 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("/api/register/admin")]
        public async Task<ActionResult<User>> RegisterAdmin(Register model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Problem("user already exists");

            User user = new()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return Problem("failed to add used to db");
             }
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.Employee))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Employee));
            if (!await _roleManager.RoleExistsAsync(UserRoles.Guest))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Guest));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            return Ok(user);
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                authClaims,
                expires: DateTime.UtcNow.AddMinutes(10000),
                signingCredentials: signIn);

            return token;
        }

    }
}
