using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public sealed class GetAddressById
{
    public sealed record Request(int AddressId, int? CustomerId) : IRequest<Response>;

    public sealed class Response : Result<Address>
    {
        public static implicit operator Response(Address successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    }

    public sealed class Handler(IEntityReader<Address> _reader) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var address = request.CustomerId is int customerId
                ? await _reader.GetOneByAsync(a =>
                    a.AddressId == request.AddressId && a.CustomerId == customerId
                )
                : await _reader.GetByIdAsync(request.AddressId);

            return address is not null ? address : new NotFoundError("Address not found");
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/addresses/{addressId}",
                    async ([FromRoute] int addressId, IMediator mediator) =>
                    {
                        var response = await mediator.Send(new Request(addressId, null));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Addresses");

            app.MapGet(
                    "/api/customers/{customerId}/addresses/{addressId}",
                    async (
                        [FromRoute] int customerId,
                        [FromRoute] int addressId,
                        IMediator mediator
                    ) =>
                    {
                        var response = await mediator.Send(new Request(addressId, customerId));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Addresses");
        }
    }
}
