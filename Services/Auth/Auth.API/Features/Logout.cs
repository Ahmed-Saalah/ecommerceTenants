using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Features;

public sealed class Logout
{
    public sealed record Request(string RefreshToken) : IRequest<Response>;

    public sealed class Response : Result<Unit>
    {
        public static implicit operator Response(Unit _) => new() { Value = Unit.Value };

        public static implicit operator Response(DomainError e) => new() { Error = e };
    }

    public sealed class Handler(
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var existingToken = await tokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (existingToken is null)
            {
                return new NotFound("Refresh token not found");
            }

            if (!existingToken.IsActive)
            {
                return new BadRequestError("Token already revoked or expired");
            }

            await tokenService.RevokeRefreshTokenAsync(
                existingToken,
                httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                    ?? "unknown"
            );

            return Unit.Value;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/users/logout",
                    async ([FromBody] Request req, IMediator mediator) =>
                    {
                        var response = await mediator.Send(req);
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Users");
        }
    }
}
