using System;
using System.Collections.Generic;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Payment
    {
        public int Id { get; set; } = default!;
        public PaymentTypes Type { get; set; } = PaymentTypes.Cash;
        public DateTime Date { get; set; }
        public Cashier Cashier { get; set; } = null!;
        public List<PaymentItem> Items { get; set; } = new List<PaymentItem>();

        public Payment(DateTime date, Cashier cashier)
        {
            Date = date;
            Cashier = cashier;
        }

        protected Payment() { }
    }
}
