using Auth.API.DbContexts;
using Auth.API.Helpers;
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
        var user = request.LoginMethod switch
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
        {
            return new NotFound("Invalid username or password");
        }

        // TODO: Password / OTP check
        //var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        //if (!passwordValid)
        //    return new ValidationError("Invalid username or password");

        var (access, refresh) = await tokenService.GenerateTokensAsync(
            user,
            httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown"
        );

        // Find role (assuming one role for simplicity)
        var role = user.UserRoles?.FirstOrDefault()?.Role?.Name;

        var profile = new ProfileData(user.UserName, user.Email, user.DisplayName, user.AvatarPath);

        return new Response(access, refresh, role, profile);
    }
}
