using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;
using Store.Core.Features;

namespace Store.API.Endpoints;

public sealed class GetStoreByIdEndpoint
{
    public sealed class Endpoint : Extensions.IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/stores/{storeId}",
                    async ([FromServices] IMediator mediator, [FromRoute] int storeId) =>
                    {
                        var response = await mediator.Send(new GetStoreById.Request(storeId));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Stores");
        }
    }
}
