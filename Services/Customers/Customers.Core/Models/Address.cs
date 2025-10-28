namespace Customers.Core.Models;

public sealed class Address
{
    public int AddressId { get; set; }
    public int CustomerId { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string? State { get; set; }
    public string Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTime Timestamp { get; set; }
    public Customer? Customer { get; set; }
}
