namespace Auth.API.Models;

public sealed class UserTenant
{
    public int UserId { get; set; }
    public int TenantId { get; set; }

    DateTime Timestamp;
}
