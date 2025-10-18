using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public class GetCustomerAddresses
{
    public sealed record Request(int CustomerId) : IRequest<Response>;

    public sealed class Response : Result<Address[]>
    {
        public static implicit operator Response(Address[] addresses) =>
            new() { Value = addresses };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed class Handler(IEntityReader<Address> _reader) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var address = await _reader.FindAsync(a => a.CustomerId == request.CustomerId);

            return address.ToArray();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/customers/{customerId}/addresses",
                    async ([FromRoute] int customerId, IMediator mediator) =>
                    {
                        var response = await mediator.Send(new Request(customerId));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
