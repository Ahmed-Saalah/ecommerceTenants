using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class GetAddressByIdEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/addresses/{addressId}",
                    async ([FromRoute] int addressId, IMediator mediator) =>
                    {
                        var response = await mediator.Send(
                            new GetAddressById.Request(addressId, null)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");

            app.MapGet(
                    "/api/customers/{customerId}/addresses/{addressId}",
                    async (
                        [FromRoute] int customerId,
                        [FromRoute] int addressId,
                        IMediator mediator
                    ) =>
                    {
                        var response = await mediator.Send(
                            new GetAddressById.Request(addressId, customerId)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
