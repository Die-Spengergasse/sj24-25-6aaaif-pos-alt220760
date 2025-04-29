using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using Microsoft.EntityFrameworkCore.InMemory;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test.PaymentTests
{
    [Collection("PaymentServiceTests")]
    public class PaymentServiceTests
    {
        private AppointmentContext CreateTestDbContext()
        {
            var dbName = $"paymenttest_{Guid.NewGuid()}";
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var db = new AppointmentContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        private void SeedDatabase(AppointmentContext db)
        {
            // CashDesk
            var cashDesk = new CashDesk(1000);
            db.CashDesks.Add(cashDesk);

            // Manager
            var manager = new Manager(
                1001, "Max", "Mustermann",
                new DateOnly(1980, 1, 1), 5000M,
                null, "BMW X7");
            manager.Type = "Manager";
            db.Managers.Add(manager);

            // Cashier
            var cashier = new Cashier(
                1002, "Anna", "Beispiel",
                new DateOnly(1990, 5, 15), 2500M,
                null, "Kassensysteme");
            cashier.Type = "Cashier";
            db.Cashiers.Add(cashier);

            db.SaveChanges();
        }

        #region CreatePayment Tests

        [Fact]
        public void CreatePaymentSuccessTest()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var cmd = new NewPaymentCommand(1000, "Cash", 1001);

            // Act
            var payment = service.CreatePayment(cmd);

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(1000, payment.CashDesk.Number);
            Assert.Equal(1001, payment.Employee.RegistrationNumber);
            Assert.Equal(PaymentType.Cash, payment.PaymentType);
            Assert.Null(payment.Confirmed);
        }

           


        [Theory]
        [InlineData(999, "Cash", 1001, "Invalid cash desk")]
        [InlineData(1000, "Cash", 999, "Invalid employee")]
        [InlineData(1000, "InvalidType", 1001, "Invalid payment type")]
        [InlineData(1000, "CreditCard", 1002, "Insufficient rights to create a credit card payment.")]
        public void CreatePaymentExceptionsTest(int cashDeskNumber, string paymentType,
            int employeeRegistrationNumber, string expectedErrorMessage)
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var cmd = new NewPaymentCommand(cashDeskNumber, paymentType, employeeRegistrationNumber);

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() => service.CreatePayment(cmd));
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void CreatePayment_OpenPaymentExists_ShouldThrowException()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);

            // Create first payment
            service.CreatePayment(new NewPaymentCommand(1000, "Cash", 1001));

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() =>
                service.CreatePayment(new NewPaymentCommand(1000, "Cash", 1001)));
            Assert.Equal("Open payment for cashdesk.", exception.Message);
        }

        #endregion

        #region ConfirmPayment Tests

        [Fact]
        public void ConfirmPayment_ValidPayment_ShouldConfirm()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var payment = service.CreatePayment(new NewPaymentCommand(1000, "Cash", 1001));

            // Act
            service.ConfirmPayment(payment.Id);

            // Assert
            var confirmedPayment = db.Payments.Find(payment.Id);
            Assert.NotNull(confirmedPayment);
            Assert.NotNull(confirmedPayment.Confirmed);
        }

        [Fact]
        public void ConfirmPayment_InvalidId_ShouldThrowException()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(999));
            Assert.Equal("Payment not found.", exception.Message);
        }

        [Fact]
        public void ConfirmPayment_AlreadyConfirmed_ShouldThrowException()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var payment = service.CreatePayment(new NewPaymentCommand(1000, "Cash", 1001));
            service.ConfirmPayment(payment.Id);

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(payment.Id));
            Assert.Equal("Payment already confirmed.", exception.Message);
        }

        #endregion

        #region AddPaymentItem Tests

        [Fact]
        public void AddPaymentItem_ValidData_ShouldAddItem()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var payment = service.CreatePayment(new NewPaymentCommand(1000, "Cash", 1001));
            var cmd = new NewPaymentItemCommand
            {
                ArticleName = "Test Article",
                Amount = 2,
                Price = 10.99m,
                PaymentId = payment.Id
            };

            // Act
            service.AddPaymentItem(cmd);

            // Assert
            var paymentWithItems = db.Payments
                .Include(p => p.PaymentItems)
                .FirstOrDefault(p => p.Id == payment.Id);
            Assert.NotNull(paymentWithItems);
            Assert.NotNull(paymentWithItems.PaymentItems);
            Assert.Single(paymentWithItems.PaymentItems);

            var paymentItem = paymentWithItems.PaymentItems.First();
            Assert.NotNull(paymentItem);
            Assert.Equal("Test Article", paymentItem.ArticleName);
            Assert.Equal(2, paymentItem.Amount);
            Assert.Equal(10.99m, paymentItem.Price);
        }

        [Fact]
        public void AddPaymentItem_InvalidPaymentId_ShouldThrowException()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand
            {
                ArticleName = "Test Article",
                Amount = 2,
                Price = 10.99m,
                PaymentId = 999
            };

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd));
            Assert.Equal("Payment not found.", exception.Message);
        }

        [Fact]
        public void AddPaymentItem_PaymentConfirmed_ShouldThrowException()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);

            // Zahlungen erstellen und bestätigen
            var cmd = new NewPaymentCommand(1000, "Cash", 1001);
            var payment = service.CreatePayment(cmd);

            // Bestätigen der Zahlung
            service.ConfirmPayment(payment.Id);

            // Neue PaymentItemCommand erstellen
            var itemCmd = new NewPaymentItemCommand
            {
                ArticleName = "Test Article",
                Amount = 2,
                Price = 10.99m,
                PaymentId = payment.Id
            };

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(itemCmd));
            Assert.Equal("Payment already confirmed.", exception.Message);
        }


        #endregion

        #region DeletePayment Tests

        [Fact]
        public void DeletePayment_WithItems_ShouldDelete()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var payment = service.CreatePayment(new NewPaymentCommand(1000, "Cash", 1001));
            var cmd = new NewPaymentItemCommand
            {
                ArticleName = "Test Article",
                Amount = 2,
                Price = 10.99m,
                PaymentId = payment.Id
            };
            service.AddPaymentItem(cmd);

            // Act
            service.DeletePayment(payment.Id, true);

            // Assert
            Assert.Null(db.Payments.FirstOrDefault(p => p.Id == payment.Id));
            Assert.Empty(db.PaymentItems.Where(pi => pi.Payment.Id == payment.Id).ToList());
        }

        [Fact]
        public void DeletePayment_WithoutItems_ShouldNotDeleteItems()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);
            var payment = service.CreatePayment(new NewPaymentCommand(1000, "Cash", 1001));
            var cmd = new NewPaymentItemCommand
            {
                ArticleName = "Test Article",
                Amount = 2,
                Price = 10.99m,
                PaymentId = payment.Id
            };
            service.AddPaymentItem(cmd);

            // Act & Assert - sollte Exception werfen wegen FK-Constraint
            var exception = Assert.Throws<PaymentServiceException>(() =>
                service.DeletePayment(payment.Id, false));

            // Wenn keine Exception geworfen wird, sollte das Payment immer noch existieren
            var paymentAfterDeletion = db.Payments.FirstOrDefault(p => p.Id == payment.Id);
            Assert.NotNull(paymentAfterDeletion);
        }


        [Fact]
        public void DeletePayment_InvalidId_ShouldReturnSilently()
        {
            // Arrange
            using var db = CreateTestDbContext();
            SeedDatabase(db);
            var service = new PaymentService(db);

            // Act & Assert - should not throw exception
            service.DeletePayment(999, true);
        }

        #endregion
    }
}
