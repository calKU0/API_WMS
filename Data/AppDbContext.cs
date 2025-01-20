using APIWMS.Models;
using Microsoft.EntityFrameworkCore;

namespace APIWMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }
        public DbSet<ApiLog> ApiLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja domyślnej wartości dla CreatedDate (aktualna data)
            modelBuilder.Entity<ApiLog>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            // Zmiana default schema
            modelBuilder.HasDefaultSchema("kkur");
        }

    }
}
