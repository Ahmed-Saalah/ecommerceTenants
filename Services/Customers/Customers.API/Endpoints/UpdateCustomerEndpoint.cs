using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class UpdateCustomerEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "/api/customers/{customerId}",
                    async (
                        [FromServices] IMediator mediator,
                        [FromRoute] int customerId,
                        [FromBody] UpdateCustomer.RequestDto data
                    ) =>
                    {
                        var response = await mediator.Send(
                            new UpdateCustomer.Request(customerId, data)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customers");
        }
    }
}
