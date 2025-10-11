using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Models;
using Auth.API.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Features;

public sealed class RefreshToken
{
    public sealed record Request(string RefreshToken) : IRequest<Response>;

    public sealed class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto success) => new() { Value = success };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed record ResponseDto(string AccessToken, string RefreshToken);

    public sealed class Handler(UserManager<User> userManager, ITokenService tokenService)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var principal = tokenService.GetPrincipalFromExpiredToken(request.RefreshToken);

            if (principal is null)
            {
                return new UnauthorizedError("Invalid refresh token.");
            }

            var userName = principal.Identity?.Name;
            if (userName is null)
            {
                return new UnauthorizedError("Invalid token payload.");
            }

            var user = await userManager.FindByNameAsync(userName);
            if (user is null)
            {
                return new NotFound("User not found.");
            }

            var isValid = await tokenService.ValidateRefreshTokenAsync(user, request.RefreshToken);
            if (!isValid)
            {
                return new UnauthorizedError("Refresh token is invalid or expired.");
            }

            var (access, refresh) = await tokenService.GenerateTokensAsync(user, "refresh");

            return new ResponseDto(access, refresh);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/auth/refresh",
                    async ([FromBody] Request request, IMediator mediator) =>
                    {
                        var response = await mediator.Send(request);
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Auth");
        }
    }
}
