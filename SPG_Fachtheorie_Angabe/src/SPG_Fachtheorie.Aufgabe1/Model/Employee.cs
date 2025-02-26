namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public abstract class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string RegistrationNumber { get; set; } = null!; 
        public Address Address { get; set; } = new Address();
        public string Type { get; private set; } = null!;

        protected Employee() { }
        public Employee(Address address)
        {
            Address = address;
        }

        protected Employee(string firstName, string lastName, string registrationNumber, Address address)
        {
            FirstName = firstName;
            LastName = lastName;
            RegistrationNumber = registrationNumber;
            Address = address;
        }
    }
}
