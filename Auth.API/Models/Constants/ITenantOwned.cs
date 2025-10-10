namespace Auth.API.Models.Constants;

/// <summary>
/// Indicates that the entity is owned by a specific tenant.
/// </summary>
public interface ITenantOwned
{
    int TenantId { get; set; }
}
