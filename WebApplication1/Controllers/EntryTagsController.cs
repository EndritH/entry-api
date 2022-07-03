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
    [Authorize]
    [ApiController]
    public class EntryTagsController : ControllerBase
    {
        private readonly EntryDbContext _context;

        private readonly UserManager<User> _userManager;

        public EntryTagsController(EntryDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/EntryTags
        [HttpGet]
        public async Task<IActionResult> GetEntryTags()
        {
            if (_context.EntryTags == null)
            {
                return NotFound();
            }
            return Ok(await _context.EntryTags.ToListAsync());
        }

        // POST: api/EntryTags
        // create entry tags 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EntryTag>> CreateEntryTag()
        {
            if (_context.EntryTags == null)
            {
                return Problem("Entity set 'EntryDbContext.EntryTags'  is null.");
            }
            var userName = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var entryTag = new EntryTag()
            {
                ActivationDate = null,
                Status = true
            };
            entryTag.User = user;
            _context.EntryTags.Add(entryTag);
            await _context.SaveChangesAsync();
            return Ok(entryTag);
            return CreatedAtAction("GetEntryTag", new { id = entryTag.Id }, entryTag);
        }

        // GET: api/EntryTags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EntryTag>> GetEntryTag(int id)
        {
            if (_context.EntryTags == null)
            {
                return NotFound();
            }
            var entryTag = await _context.EntryTags.FindAsync(id);

            if (entryTag == null)
            {
                return NotFound();
            }
            return entryTag;
        }

        // PUT: api/EntryTags/5
        // Authorize entry tag 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost()]
        [Authorize(Roles=UserRoles.Admin)]
        [Route("{id}/authorize")]
        public async Task<IActionResult> AuthorizeEntryTag(int id)
        {
            if (_context.EntryTags == null)
            {
                return NotFound();
            }
            var entryTag = await _context.EntryTags.FindAsync(id);

            if (entryTag == null)
            {
                return NotFound();
            }
            if(entryTag.ActivationDate!=null && entryTag.Status == false)
            {
                return BadRequest("can not authorize this status code");
            }

            try
            {
                entryTag.ActivationDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(entryTag);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntryTagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/EntryTags/5
        // Authorize entry tag 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost()]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{id}/deactivate")]
        public async Task<IActionResult> DeactivateEntryTag(int id)
        {
            if (_context.EntryTags == null)
            {
                return NotFound();
            }
            var entryTag = await _context.EntryTags.FindAsync(id);

            if (entryTag == null)
            {
                return NotFound();
            }

            try
            {
                entryTag.Status = false;
                await _context.SaveChangesAsync();
                return Ok(entryTag);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntryTagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool EntryTagExists(int id)
        {
            return (_context.EntryTags?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
