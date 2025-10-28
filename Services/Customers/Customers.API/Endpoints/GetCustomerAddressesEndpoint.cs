using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public class GetCustomerAddressesEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/customers/{customerId}/addresses",
                    async ([FromRoute] int customerId, [FromServices] IMediator mediator) =>
                    {
                        var response = await mediator.Send(
                            new GetCustomerAddresses.Request(customerId)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
