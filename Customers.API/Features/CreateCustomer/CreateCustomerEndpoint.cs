using Auth.API.Extensions;
using Customers.API.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features.CreateCustomer;

public class CreateCustomerEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/customers",
                async (IMediator mediator, [FromBody] CreateCustomerRequest data) =>
                {
                    var response = await mediator.Send(data);
                    return response.ToHttpResult();
                }
            )
            .WithTags("Users");
    }
}
