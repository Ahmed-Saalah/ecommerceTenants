namespace Auth.API.Models.Constants;

public sealed class RoleConstants
{
    public const string Tenant = "tenant"; // Global system administrator
    public const string TenantManager = "tenantManager"; // Manages store/tenant
    public const string Customer = "customer"; // Buyer/user of the store
    public const string Guest = "guest"; // Not registered/logged-in
}
