using System;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class PaymentItem
    {
        protected PaymentItem() { } // Für EF Core

        public PaymentItem(string articleName, int amount, decimal price, Payment payment)
        {
            ArticleName = articleName ?? throw new ArgumentNullException(nameof(articleName));
            Amount = amount;
            Price = price;
            Payment = payment ?? throw new ArgumentNullException(nameof(payment));
        }

        public int Id { get; set; }
        public string ArticleName { get; set; } = null!;
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public Payment Payment { get; set; } = null!;
        public DateTime? LastUpdated { get; set; }
    }
}
