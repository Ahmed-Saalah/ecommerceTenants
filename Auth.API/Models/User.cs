using Microsoft.AspNetCore.Identity;

namespace Auth.API.Models;

public sealed class User : IdentityUser<int>
{
    public string? DisplayName { get; set; }
    public string? AvatarPath { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public DateTime? LoggedInAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
