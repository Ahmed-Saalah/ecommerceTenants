using Auth.API.Extensions;
using Auth.API.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Features.Login;

public sealed class LoginEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/users/login",
                async ([FromBody] LoginRequest request, [FromServices] IMediator mediator) =>
                {
                    var response = await mediator.Send(request);
                    return response.ToHttpResult();
                }
            )
            .WithTags("Users");
    }
}
