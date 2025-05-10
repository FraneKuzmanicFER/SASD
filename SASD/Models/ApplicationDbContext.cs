using Microsoft.EntityFrameworkCore;

namespace SASD.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Sport> Sports { get; set; }
        public DbSet<SportEvent> SportEvents { get; set; }
        public DbSet<PlayerRecord> PlayerRecords { get; set; }
    }
}
