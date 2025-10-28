using Core.DataAccess.Abstractions;
using Customers.Core.Models;
using MediatR;
using Shared.Web.Helpers;

namespace Customers.Core.Features;

public sealed class CreateCustomer
{
    public sealed record Request(
        int UserId,
        string Username,
        string? Email,
        string? PhoneNumber,
        string DisplayName
    ) : IRequest<Response>;

    public sealed record ResponseDto(
        int CustomerId,
        string Username,
        string? Email,
        string? PhoneNumber,
        string DisplayName
    );

    public sealed class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto response) =>
            new() { Value = response };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed class Handler(IEntityWriter<Customer> customerWriter)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                UserId = request.UserId,
                Username = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DisplayName = request.DisplayName,
                Timestamp = DateTime.UtcNow,
                AddressId = null,
            };

            await customerWriter.UpsertAsync(customer, c => c.UserId);

            return new ResponseDto(
                customer.CustomerId,
                customer.Username,
                customer.Email,
                customer.PhoneNumber,
                customer.DisplayName
            );
        }
    }
}
