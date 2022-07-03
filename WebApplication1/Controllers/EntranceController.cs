using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Auth;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EntranceController : ControllerBase
    {
        private readonly EntryDbContext _context;
        private readonly UserManager<User> _userManager;
        public EntranceController(EntryDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // PUT: api/Entrance
        // Authorize entry tag 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost()]
        public async Task<IActionResult> Post()
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var roles = await _userManager.GetRolesAsync(user);
            var tags = await _context.EntryTags.Where(t => t.User.Id == user.Id && t.Status == true && t.ActivationDate != null).ToListAsync();
            if (tags != null)
            {
                if (roles.Contains(UserRoles.Admin))
                {
                    if (tags.Where(e => e.wasActivatedBeforeNow() && e.wasNotActivatedBefore(DateTime.Now.AddYears(-3))).Any())
                    {
                        return Ok("valid entry for " + user.UserName);
                    }
                }
                else if (roles.Contains(UserRoles.Employee))
                {
                    if (tags.Where(e => e.wasActivatedBeforeNow() && e.wasNotActivatedBefore(DateTime.Now.AddYears(-1))).Any())
                    {
                        return Ok("valid entry for "+user.UserName);
                    }
                }
                else if (roles.Contains(UserRoles.Guest))
                {
                    if (tags.Where(e => e.wasActivatedBeforeNow() && e.wasNotActivatedBefore(DateTime.Now.AddHours(-1))).Any())
                    {
                        return Ok("valid entry for " + user.UserName);
                    }
                }
            }
            return Problem("you do not have an valid tag");
        }
    }
}
