using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;

namespace SPG_Fachtheorie.Aufgabe1.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Cashier> Cashiers { get; set; } = null!;
        public DbSet<Manager> Managers { get; set; } = null!;
        public DbSet<CashDesk> CashDesks { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<PaymentItem> PaymentItems { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasDiscriminator<string>("Type")
                .HasValue<Cashier>("Cashier")
                .HasValue<Manager>("Manager");

            modelBuilder.Entity<Address>()
                .OwnsOne(a => a); 

            base.OnModelCreating(modelBuilder);
        }
    }
}
