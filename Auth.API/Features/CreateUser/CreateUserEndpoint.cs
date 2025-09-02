using Auth.API.Extensions;
using Auth.API.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Features.CreateUser;

public sealed class CreateUserEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/users",
                async ([FromBody] CreateUserRequest request, [FromServices] IMediator mediator) =>
                {
                    var response = await mediator.Send(request);
                    return response.ToHttpResult();
                }
            )
            .WithTags("Users");
    }
}
