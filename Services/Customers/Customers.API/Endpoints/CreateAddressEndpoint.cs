using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class CreateAddressEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/customers/{customerId}/address",
                    async (
                        [FromServices] IMediator mediator,
                        [FromRoute] int customerId,
                        [FromBody] CreateAddress.RequestDto data
                    ) =>
                    {
                        var response = await mediator.Send(
                            new CreateAddress.Request(customerId, data)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
