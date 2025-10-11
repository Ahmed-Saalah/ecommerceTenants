using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Models;
using Core.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Features;

public sealed class GetCurrentUser
{
    public sealed record Request() : IRequest<Response>;

    public sealed class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto success) => new() { Value = success };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed record ResponseDto(
        int Id,
        string Username,
        string Email,
        string DisplayName,
        string Role
    );

    public sealed class Handler(UserManager<User> userManager, IUserContext userContext)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var userId = userContext.UserId;
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return new NotFound("User not found.");
            }

            var roles = await userManager.GetRolesAsync(user);

            return new ResponseDto(
                user.Id,
                user.UserName!,
                user.Email!,
                user.DisplayName!,
                roles.FirstOrDefault() ?? "None"
            );
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/users/me",
                    async (IMediator mediator) =>
                    {
                        var response = await mediator.Send(new Request());
                        return response.ToHttpResult();
                    }
                )
                .RequireAuthorization()
                .WithTags("Users");
        }
    }
}
