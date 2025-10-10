using Auth.API.Models.Constants;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Models;

public sealed class User : IdentityUser<int>, ITenantOwned
{
    public string? DisplayName { get; set; }
    public string? AvatarPath { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public DateTime? LoggedInAt { get; set; }
    public int TenantId { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
