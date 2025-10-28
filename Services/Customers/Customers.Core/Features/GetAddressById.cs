using Core.DataAccess;
using Customers.Core.Models;
using MediatR;
using Shared.Web.Helpers;

namespace Customers.Core.Features;

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

            return address is not null ? address : new NotFound("Address not found");
        }
    }
}
