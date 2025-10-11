using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Models;
using Auth.API.Models.Constants;
using Auth.API.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Features;

public sealed class CreateGuest
{
    public sealed record Request(
        string Username,
        string? Email,
        string PhoneNumber,
        string Password,
        string DisplayName,
        string Role,
        string? AvatarPath,
        UserClaim[]? Claims
    ) : IRequest<Response>;

    public sealed class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    }

    public sealed record ResponseDto(int UserId, string AccessToken, string RefreshToken);

    public sealed class Handler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
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
                httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                    ?? "unknown"
            );

            return new ResponseDto(user.Id, access, refresh);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/users/guest",
                    async (IMediator mediator, [FromQuery] int[]? tenantIds) =>
                    {
                        var response = await mediator.Send(
                            new Request(
                                Username: GuestData.Username,
                                Email: GuestData.Email,
                                PhoneNumber: GuestData.PhoneNumber,
                                Password: GuestData.Password,
                                DisplayName: GuestData.DisplayName,
                                Role: RoleConstants.Guest,
                                Claims: [],
                                AvatarPath: null
                            )
                        );

                        return response.ToHttpResult();
                    }
                )
                .WithTags("Users");
        }
    }
}
