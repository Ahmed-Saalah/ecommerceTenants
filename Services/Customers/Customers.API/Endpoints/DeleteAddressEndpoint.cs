using Customers.API.Extensions;
using Customers.Core.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;

namespace Customers.API.Endpoints;

public sealed class DeleteAddressEndpoint
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "/api/addresses/{addressId}",
                    async ([FromRoute] int addressId, [FromServices] IMediator mediator) =>
                    {
                        var response = await mediator.Send(new DeleteAddress.Request(addressId));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
