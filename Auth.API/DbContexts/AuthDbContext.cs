using Auth.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.DbContexts;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<
        User,
        Role,
        int,
        IdentityUserClaim<int>,
        UserRole,
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>,
        IdentityUserToken<int>
    >(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder
            .Entity<User>()
            .HasMany(u => u.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);

        builder.Entity<User>().HasIndex(u => u.TenantId);

        builder
            .Entity<Role>()
            .HasMany(u => u.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId);

        builder
            .Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId);

        builder
            .Entity<IdentityRole<int>>()
            .HasData(
                new IdentityRole<int>
                {
                    Id = 1,
                    Name = "admin",
                    NormalizedName = "ADMIN",
                },
                new IdentityRole<int>
                {
                    Id = 2,
                    Name = "owner",
                    NormalizedName = "OWNER",
                },
                new IdentityRole<int>
                {
                    Id = 3,
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                },
                new IdentityRole<int>
                {
                    Id = 4,
                    Name = "guest",
                    NormalizedName = "GUEST",
                }
            );
    }
}
