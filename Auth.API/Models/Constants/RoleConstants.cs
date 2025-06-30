namespace Auth.API.Models.Constants;

public sealed class RoleConstants
{
    public const string Admin = "admin"; // Global system administrator
    public const string StoreOwner = "owner"; // Manages a single store/tenant
    public const string Customer = "customer"; // Buyer/user of the store
    public const string Guest = "guest"; // Not registered/logged-in
}
