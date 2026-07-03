using Microsoft.EntityFrameworkCore;
using DBPQRPermanent.Models;

namespace DBPQRPermanent.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Employee>().HasKey(e => e.EmpId);
            modelBuilder.Entity<QRCode>().HasIndex(q => q.EmpId).IsUnique();
        }
    }
}
