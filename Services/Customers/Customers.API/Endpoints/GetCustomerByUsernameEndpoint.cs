using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class GetCustomerByUsernameEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/customers/{username}",
                    async (IMediator mediator, [FromRoute] string username) =>
                    {
                        var response = await mediator.Send(
                            new GetCustomerByUsername.Request(username)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customers");
        }
    }
}
