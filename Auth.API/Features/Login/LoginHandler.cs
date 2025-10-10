using Auth.API.DbContexts;
using Auth.API.Helpers;
using Auth.API.Models;
using Auth.API.Models.Constants;
using Auth.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Features.Login;

public sealed class LoginHandler(
    AuthDbContext dbContext,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<LoginRequest, Result<Response>>
{
    public async Task<Result<Response>> Handle(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        // Lookup user (username or phone login)
        User? user = request.LoginMethod switch
        {
            LoginMethods.Phone => await dbContext
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.Username, cancellationToken),

            _ => await dbContext
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken),
        };

        if (user is null)
            return new NotFound("Invalid username or password");

        // TODO: Password / OTP check
        //var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        //if (!passwordValid)
        //    return new ValidationError("Invalid username or password");

        var (access, refresh) = await tokenService.GenerateTokensAsync(
            user,
            httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown"
        );

        // Find role (assuming one role for simplicity)
        var role = user.UserRoles?.FirstOrDefault()?.Role?.Name ?? string.Empty;

        var profile = new ProfileData(
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.DisplayName ?? string.Empty,
            user.AvatarPath ?? string.Empty
        );

        return new Response(access, refresh, role, profile);
    }
}
