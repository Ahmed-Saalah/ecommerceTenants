using Auth.API.Helpers;
using Auth.API.Models;
using Auth.API.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Mediator;

public sealed class CreateGuest
{
    public sealed record ResponseDto(
        int UserId,
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresIn
    );

    public class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    }

    public sealed record Request(
        string Username,
        string? Email,
        string PhoneNumber,
        string Password,
        string DisplayName,
        string Role,
        string? AvatarPath,
        int[]? TenantIds,
        UserClaim[]? Claims
    ) : IRequest<Response>;

    public sealed class Handler(UserManager<User> userManager, ITokenGenerator tokenGenerator)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                UserName = request.Username,
                Email = request.Email,
                DisplayName = request.DisplayName ?? request.Username,
                PhoneNumber = request.PhoneNumber,
                AvatarPath = request.AvatarPath,
                RegisteredAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return new ValidationError(result.Errors);

            await userManager.AddToRoleAsync(user, request.Role);

            var (accessToken, refreshToken, expiresAt) = tokenGenerator.Generate(user);
            return new ResponseDto(user.Id, accessToken, refreshToken, expiresAt);
        }
    }
}
