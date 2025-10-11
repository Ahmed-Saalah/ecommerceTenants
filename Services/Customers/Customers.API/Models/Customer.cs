namespace Customers.API.Models;

public sealed class Customer
{
    public int CustomerId { get; set; }

    public string Username { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? DisplayName { get; set; }

    public DateTime Timestamp { get; set; }

    public int? AddressId { get; set; }

    public ICollection<Address>? Addresses { get; set; } = new List<Address>();
    public ICollection<CustomerTenant>? CustomerTenants { get; set; } = new List<CustomerTenant>();
}
