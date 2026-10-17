using Microsoft.EntityFrameworkCore;
using IntelSphere.API.Models;

namespace IntelSphere.API.Data
{
    public class AppDbContext : DbContext
    {
        // Bu constructor, Program.cs tarafındaki veritabanı ayarlarını EF Core'a taşır.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanındaki tablolarımızın C# karşılıkları (DbSet)
        public DbSet<BreachSource> BreachSources { get; set; }
        public DbSet<MonitoredDomain> MonitoredDomains { get; set; }
        public DbSet<LeakedCredential> LeakedCredentials { get; set; }

        // İlişkisel kuralları ve veritabanı yapılandırmasını ince ayarladığımız yer
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // LeakedCredential ile BreachSource arasındaki 1-to-Many (Bire-Çok) ilişkiyi netleştiriyoruz
            modelBuilder.Entity<LeakedCredential>()
                .HasOne(lc => lc.BreachSource)
                .WithMany()
                .HasForeignKey(lc => lc.BreachSourceId)
                .OnDelete(DeleteBehavior.Cascade); // Kaynak silinirse, ona ait sızıntı kayıtları da silinsin.

            // Büyük veride arama performansını optimize etmek için Domain alanına indeks atıyoruz.
            modelBuilder.Entity<LeakedCredential>()
                .HasIndex(lc => lc.Domain);
        }
    }
}