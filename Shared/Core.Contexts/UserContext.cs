using Microsoft.AspNetCore.Http;

namespace Core.Contexts;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private const string UserIdClaim = "user_id";
    private const string TenantIdClaim = "tenant_id";

    public int UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(UserIdClaim)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
            }
            return userId;
        }
    }

    public int TenantId
    {
        get
        {
            var tenantIdClaim = httpContextAccessor
                .HttpContext?.User?.FindFirst(TenantIdClaim)
                ?.Value;
            if (
                string.IsNullOrEmpty(tenantIdClaim)
                || !int.TryParse(tenantIdClaim, out var tenantId)
            )
                throw new UnauthorizedAccessException("Tenant ID claim is missing or invalid.");
            return tenantId;
        }
    }
}
