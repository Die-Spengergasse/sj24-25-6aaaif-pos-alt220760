using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    [Collection("Sequential")]
    public class Aufgabe1Test
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseSqlite(@"Data Source=cash.db")
                .Options;

            var db = new AppointmentContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return db;
        }

        // Erstellt eine leere Datenbank in Debug\net8.0\cash.db
        [Fact]
        public void CreateDatabaseTest()
        {
            using var db = GetEmptyDbContext();
        }

        // Test: Cashier 
        [Fact]
        public void AddCashierSuccessTest()
        {
            using var db = GetEmptyDbContext();

            var address = new Address { Street = "Hauptstraße 1", City = "Wien", ZipCode = "1010" };
            var cashDesk = new CashDesk { Location = "Eingangshalle" };

            var cashier = new Cashier("Max", "Mustermann", "C123", address, cashDesk);

            db.Cashiers.Add(cashier);
            db.SaveChanges();

            var result = db.Cashiers.SingleOrDefault(c => c.RegistrationNumber == "C123");

            Assert.NotNull(result);
            Assert.Equal("Max", result.FirstName);
            Assert.Equal("Mustermann", result.LastName);
            Assert.Equal("C123", result.RegistrationNumber);
        }

        // Test: Zahlung 
        [Fact]
        public void AddPaymentSuccessTest()
        {
            using var db = GetEmptyDbContext();

            var address = new Address { Street = "Marktplatz 2", City = "Graz", ZipCode = "8010" };
            var cashDesk = new CashDesk { Location = "Kasse 2" };
            var cashier = new Cashier("Anna", "Müller", "C456", address, cashDesk);

            db.Cashiers.Add(cashier);
            db.SaveChanges();

            var payment = new Payment(DateTime.Now, cashier);
            db.Payments.Add(payment);
            db.SaveChanges();

            var result = db.Payments.SingleOrDefault(p => p.Cashier.RegistrationNumber == "C456");

            Assert.NotNull(result);
            Assert.Equal(cashier.Id, result.Cashier.Id);
        }

        // Test: Überprüfung des Employee-Discriminators
        [Fact]
        public void EmployeeDiscriminatorSuccessTest()
        {
            using var db = GetEmptyDbContext();
          
          

            var address = new Address { Street = "Hauptstraße 5", City = "Salzburg", ZipCode = "5020" };

            var cashier = new Cashier("Maria", "Schmidt", "C789", address, new CashDesk { Location = "Kasse 3" });
            var manager = new Manager("Thomas", "Kaiser", "M111", address, "Verkauf");

            db.Employees.Add(cashier);
            db.Employees.Add(manager);
            db.SaveChanges();

            var resultCashier = db.Employees.OfType<Cashier>().SingleOrDefault(c => c.RegistrationNumber == "C789");
            var resultManager = db.Employees.OfType<Manager>().SingleOrDefault(m => m.RegistrationNumber == "M111");

            Assert.NotNull(resultCashier);
            Assert.NotNull(resultManager);
            Assert.Equal("Maria", resultCashier.FirstName);
            Assert.Equal("Thomas", resultManager.FirstName);
        }
    }
}
