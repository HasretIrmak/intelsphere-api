using Microsoft.EntityFrameworkCore;
using IntelSphere.API.Models;

namespace IntelSphere.API.Data
{
    public class AppDbContext : DbContext
    {
        // This constructor passes the database settings from Program.cs to EF Core.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // C# counterparts (DbSets) of our database tables
        public DbSet<BreachSource> BreachSources { get; set; }
        public DbSet<MonitoredDomain> MonitoredDomains { get; set; }
        public DbSet<LeakedCredential> LeakedCredentials { get; set; }

        // Where we fine-tune relational rules and database configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Clarifying the 1-to-Many relationship between LeakedCredential and BreachSource
            modelBuilder.Entity<LeakedCredential>()
                .HasOne(lc => lc.BreachSource)
                .WithMany()
                .HasForeignKey(lc => lc.BreachSourceId)
                .OnDelete(DeleteBehavior.Cascade); // If the source is deleted, its related leak records will also be deleted.

            // Assigning an index to the Domain field to optimize search performance in large data.
            modelBuilder.Entity<LeakedCredential>()
                .HasIndex(lc => lc.Domain);
        }
    }
}