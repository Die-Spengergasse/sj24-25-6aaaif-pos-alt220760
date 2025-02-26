namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Address
    {
        public string Id { get; set; } = default!;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;

        public Address(string street, string city, string zipCode)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
        }

        public Address() { }
    }
}
