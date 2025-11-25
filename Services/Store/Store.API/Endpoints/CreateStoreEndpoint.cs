using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Helpers;
using Store.Core.Features;

namespace Store.API.Endpoints;

public sealed class CreateStoreEndpoint
{
    public sealed class Endpoint : Extensions.IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/stores",
                    async (
                        [FromServices] IMediator mediator,
                        [FromRoute] int customerId,
                        [FromBody] CreateStore.Request data
                    ) =>
                    {
                        var response = await mediator.Send(data);
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
