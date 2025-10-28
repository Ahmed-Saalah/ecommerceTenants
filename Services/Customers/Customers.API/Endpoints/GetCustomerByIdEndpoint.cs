using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class GetCustomerByIdEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/customers/{customerId:int}",
                    async ([FromServices] IMediator mediator, [FromRoute] int customerId) =>
                    {
                        var response = await mediator.Send(new GetCustomerById.Request(customerId));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customers");
        }
    }
}
