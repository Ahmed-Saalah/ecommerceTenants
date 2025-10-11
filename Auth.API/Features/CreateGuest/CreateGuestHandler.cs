using Auth.API.Helpers;
using Auth.API.Models;
using Auth.API.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Features.CreateGuest;

public sealed class CreateGuestHandler(
    UserManager<User> userManager,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<CreateGuestRequest, CreateGuestResponse>
{
    public async Task<CreateGuestResponse> Handle(
        CreateGuestRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            DisplayName = request.DisplayName,
            PhoneNumber = request.PhoneNumber,
            AvatarPath = request.AvatarPath,
            RegisteredAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new ValidationError(result.Errors);
        }

        await userManager.AddToRoleAsync(user, request.Role);

        var (access, refresh) = await tokenService.GenerateTokensAsync(
            user,
            httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown"
        );

        return new ResponseDto(user.Id, access, refresh);
    }
}
