using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;

namespace SPG_Fachtheorie.Aufgabe1.Infrastructure
{
    public class AppointmentContext : DbContext
    {
        public AppointmentContext(DbContextOptions<AppointmentContext> options)
            : base(options) { }

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Cashier> Cashiers { get; set; } = null!;
        public DbSet<Manager> Managers { get; set; } = null!;
        public DbSet<CashDesk> CashDesks { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<PaymentItem> PaymentItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasDiscriminator<string>("Type")
                .HasValue<Cashier>("Cashier")
                .HasValue<Manager>("Manager");

            modelBuilder.Entity<Employee>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Employee>()
                .Property(e => e.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Employee>()
                .OwnsOne(e => e.Address);

            base.OnModelCreating(modelBuilder);
        }
    }
}
