public class CashDesk
{
    public int Number { get; set; }          // PK

    public string Location { get; set; } = "DefaultLocation";

    public CashDesk(int number, string location = "DefaultLocation")
    {
        Number = number;
        Location = location;
    }

    public CashDesk() // EF Core braucht public ctor
    {
        Location = "DefaultLocation";
    }
}
