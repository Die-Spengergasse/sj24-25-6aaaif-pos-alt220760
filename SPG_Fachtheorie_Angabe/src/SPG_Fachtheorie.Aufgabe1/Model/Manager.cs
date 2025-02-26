namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Manager : Employee
    {
        public string Department { get; set; } = null!;

        public Manager(string firstName, string lastName, string registrationNumber, Address address, string department)
            : base(firstName, lastName, registrationNumber, address)
        {
            Department = department;
        }

        protected Manager() { }
    }
}
