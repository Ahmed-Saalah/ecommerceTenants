using Core.DataAccess;
using Customers.Core.Models;
using MediatR;
using Shared.Web.Helpers;

namespace Customers.Core.Features;

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
}
