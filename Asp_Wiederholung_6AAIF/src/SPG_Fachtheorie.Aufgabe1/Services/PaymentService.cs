using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class PaymentService
    {
        private readonly AppointmentContext _db;

        public PaymentService(AppointmentContext db)
        {
            _db = db;
        }

        // Expose IQueryable<Payment> for GET methods
        public IQueryable<Payment> Payments => _db.Payments;

        public Payment CreatePayment(NewPaymentCommand cmd)
        {
            var cashDesk = _db.CashDesks.FirstOrDefault(c => c.Number == cmd.CashDeskNumber);
            if (cashDesk is null) throw new PaymentServiceException("Invalid cash desk");

            var employee = _db.Employees.FirstOrDefault(e => e.RegistrationNumber == cmd.EmployeeRegistrationNumber);
            if (employee is null) throw new PaymentServiceException("Invalid employee");

            if (!Enum.TryParse<PaymentType>(cmd.PaymentType, out var paymentType))
                throw new PaymentServiceException("Invalid payment type");

            // Prüfen, ob bereits ein offenes Payment für diese Kassa existiert
            if (_db.Payments.Any(p => p.CashDesk.Number == cmd.CashDeskNumber && p.Confirmed == null))
                throw new PaymentServiceException("Open payment for cashdesk.");

            // Prüfen, ob bei CreditCard Payment der Employee ein Manager ist
            if (paymentType == PaymentType.CreditCard && employee.Type != "Manager")
                throw new PaymentServiceException("Insufficient rights to create a credit card payment.");

            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, paymentType);
            _db.Payments.Add(payment);
            _db.SaveChanges();
            return payment;
        }

        public void ConfirmPayment(int paymentId)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment is null)
                throw new PaymentServiceException("Payment not found.");

            if (payment.Confirmed.HasValue)
                throw new PaymentServiceException("Payment already confirmed.");

            payment.Confirmed = DateTime.UtcNow;
            _db.SaveChanges();
        }


        public void AddPaymentItem(NewPaymentItemCommand cmd)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == cmd.PaymentId);
            if (payment is null)
                throw new PaymentServiceException("Payment not found.");

            if (payment.Confirmed.HasValue)
                throw new PaymentServiceException("Payment already confirmed.");

            var paymentItem = new PaymentItem(cmd.ArticleName, cmd.Amount, cmd.Price, payment);
            _db.PaymentItems.Add(paymentItem);
            _db.SaveChanges();
        }

        public void DeletePayment(int paymentId, bool deleteItems)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment is null) return;

            try
            {
                if (deleteItems)
                {
                    var paymentItems = _db.PaymentItems.Where(p => p.Payment.Id == paymentId).ToList();
                    if (paymentItems.Any())
                    {
                        _db.PaymentItems.RemoveRange(paymentItems);
                    }
                }

                _db.Payments.Remove(payment);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new PaymentServiceException(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }

    public class PaymentServiceException : Exception
    {
        public PaymentServiceException(string message) : base(message) { }
    }
}
