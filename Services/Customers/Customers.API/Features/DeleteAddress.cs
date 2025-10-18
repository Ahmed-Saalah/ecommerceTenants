using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public sealed class DeleteAddress
{
    public sealed record Request(int AddressId) : IRequest<Response>;

    public sealed class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    }

    public sealed record ResponseDto(string Message);

    public sealed class Handler(IEntityWriter<Address> _writer) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            await _writer.DeleteAsync(request.AddressId);
            return new ResponseDto("Address deleted successfully");
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "/api/addresses/{addressId}",
                    async ([FromRoute] int addressId, IMediator mediator) =>
                    {
                        var response = await mediator.Send(new Request(addressId));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
