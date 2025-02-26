namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Cashier : Employee
    {
        public CashDesk CashDesk { get; set; } = null!;

        public Cashier(string firstName, string lastName, string registrationNumber, Address address, CashDesk cashDesk)
            : base(firstName, lastName, registrationNumber, address)
        {
            CashDesk = cashDesk;
        }

        protected Cashier() { }
    }
}