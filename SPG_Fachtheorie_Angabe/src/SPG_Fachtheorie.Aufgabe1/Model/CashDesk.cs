namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class CashDesk
    {
        public int Id { get; set; } = default!;
        public string Location { get; set; } = string.Empty;

        public CashDesk(string location)
        {
            Location = location;
        }

        public CashDesk() { }
    }
}
