using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public sealed class CreateCustomer
{
    public sealed record Request(
        int CustomerId,
        int TenantId,
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

    public sealed class CreateCustomerHandler(IEntityWriter<Customer> customerWriter)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                CustomerId = request.CustomerId,
                Username = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DisplayName = request.DisplayName,
                Timestamp = DateTime.UtcNow,
                AddressId = null,
            };

            await customerWriter.UpsertAsync(customer, c => c.CustomerId);

            return new ResponseDto(
                customer.CustomerId,
                customer.Username,
                customer.Email,
                customer.PhoneNumber,
                customer.DisplayName
            );
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/customers",
                    async (IMediator mediator, [FromBody] Request data) =>
                    {
                        var response = await mediator.Send(data);
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customers");
        }
    }
}
