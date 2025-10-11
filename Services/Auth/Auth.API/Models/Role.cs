using Microsoft.AspNetCore.Identity;

namespace Auth.API.Models;

public sealed class Role : IdentityRole<int>
{
    public ICollection<UserRole> UserRoles { get; set; }
}
