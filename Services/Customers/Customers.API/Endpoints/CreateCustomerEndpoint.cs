using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class CreateCustomerEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/customers",
                    async (
                        [FromServices] IMediator mediator,
                        [FromBody] CreateCustomer.Request data
                    ) =>
                    {
                        var response = await mediator.Send(data);
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customers");
        }
    }
}
