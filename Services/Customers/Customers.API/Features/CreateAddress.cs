using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public sealed class CreateAddress
{
    public class Response : Result<Address>
    {
        public static implicit operator Response(Address successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    };

    public sealed record RequestDto(
        string Street,
        string City,
        string? State,
        string Country,
        string? PostalCode
    );

    public sealed record Request(int CustomerId, RequestDto AddressInfo) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(s => s.CustomerId).GreaterThan(0).WithMessage("Customer id is invalid");

            RuleFor(s => s.AddressInfo.Country).NotEmpty().WithMessage("Country is required");

            RuleFor(s => s.AddressInfo.Street).NotEmpty().WithMessage("Street is required");
        }
    }

    public sealed class Handler(IEntityWriter<Address> _writer, Validator _validator)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new ValidationError(validationResult.Errors);
            }

            var address = new Address
            {
                CustomerId = request.CustomerId,
                Street = request.AddressInfo.Street,
                City = request.AddressInfo.City,
                State = request.AddressInfo.State,
                Country = request.AddressInfo.Country,
                PostalCode = request.AddressInfo.PostalCode,
                Timestamp = DateTime.UtcNow,
            };

            var addedAddress = await _writer.AddAsync(address);

            return addedAddress;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/customers/{customerId}/address",
                    async (
                        IMediator mediator,
                        [FromRoute] int customerId,
                        [FromBody] RequestDto data
                    ) =>
                    {
                        var response = await mediator.Send(new Request(customerId, data));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer address");
        }
    }
}
