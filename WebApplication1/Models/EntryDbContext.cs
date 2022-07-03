using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
namespace WebApplication1.Models
{
    public class EntryDbContext : IdentityDbContext<IdentityUser>
    {
        public EntryDbContext(DbContextOptions<EntryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public DbSet<EntryTag> EntryTags { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}
