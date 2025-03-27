using System;
using System.Collections.Generic;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Payment
    {
        public int Id { get; set; }

        // Falls du weiter mit "Id" in CashDesk arbeitest, kannst du es so lassen.
        // Sonst: public CashDesk CashDesk { get; set; } = null!;
        public CashDesk CashDesk { get; set; } = null!;

        public DateTime PaymentDateTime { get; set; }
        public Employee Employee { get; set; } = null!;

        // Non-nullable => Defaultwert
        public PaymentType PaymentType { get; set; } = PaymentType.Cash;

        public List<PaymentItem> PaymentItems { get; } = new();
        public DateTime? Confirmed { get; set; }

        // EF-Core-Konstruktor => Default PaymentType
        public Payment()
        {
            PaymentType = PaymentType.Cash;
        }

        // Dein normaler Konstruktor
        public Payment(CashDesk cashDesk, DateTime paymentDateTime, Employee employee, PaymentType paymentType)
        {
            CashDesk = cashDesk;
            PaymentDateTime = paymentDateTime;
            Employee = employee;
            PaymentType = paymentType;
        }
    }
}