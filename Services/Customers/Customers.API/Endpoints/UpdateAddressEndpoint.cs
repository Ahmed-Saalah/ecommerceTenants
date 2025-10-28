using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class UpdateAddressEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "/api/customers/{customerId}/address/{addressId}",
                    async (
                        IMediator mediator,
                        [FromRoute] int customerId,
                        [FromRoute] int addressId,
                        [FromBody] UpdateAddress.RequestDto data
                    ) =>
                    {
                        var response = await mediator.Send(
                            new UpdateAddress.Request(customerId, addressId, data)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
