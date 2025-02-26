namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class PaymentItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public Payment Payment { get; set; } = null!;

        public PaymentItem(string productName, int amount, decimal price, Payment payment)
        {
            ProductName = productName;
            Amount = amount;
            Price = price;
            Payment = payment;
        }

        protected PaymentItem() { }
    }
}
