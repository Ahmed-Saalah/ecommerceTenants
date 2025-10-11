namespace Customers.API.Models;

public sealed class CustomerTenant
{
    public int CustomerId { get; set; }
    public int TenantId { get; set; }
    public DateTime Timestamp { get; set; }
    public Customer? Customer { get; set; }
}
