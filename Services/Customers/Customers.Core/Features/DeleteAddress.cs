using Core.DataAccess.Abstractions;
using Customers.Core.Models;
using MediatR;
using Shared.Web.Helpers;

namespace Customers.Core.Features;

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
}
