using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModBrowser.Models;
using System.Linq;

namespace ModBrowser.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            if (this.Database.GetPendingMigrations().Any())
            {
                this.Database.Migrate();
            }
        }

        public DbSet<Mod> Mod { get; set; }
    }
}
