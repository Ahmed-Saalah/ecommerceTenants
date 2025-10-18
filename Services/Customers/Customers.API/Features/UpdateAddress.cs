using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public sealed class UpdateAddress
{
    public sealed class Response : Result<Address>
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

    public sealed record Request(int CustomerId, int AddressId, RequestDto AddressInfo)
        : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(s => s.CustomerId).GreaterThan(0).WithMessage("Customer id is invalid");

            RuleFor(s => s.AddressInfo.Country).NotEmpty().WithMessage("Country is required");

            RuleFor(s => s.AddressInfo.Street).NotEmpty().WithMessage("Street is required");
        }
    }

    public sealed class Handler(
        IEntityReader<Address> _reader,
        IEntityWriter<Address> _writer,
        Validator _validator
    ) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new ValidationError(validationResult.Errors);
            }

            var existingAddress = await _reader.GetOneByAsync(a =>
                a.CustomerId == request.CustomerId && a.AddressId == request.AddressId
            );

            if (existingAddress is null)
            {
                return new NotFoundError($"address not found.");
            }

            existingAddress.Street = request.AddressInfo.Street ?? existingAddress.Street;
            existingAddress.City = request.AddressInfo.City ?? existingAddress.City;
            existingAddress.State = request.AddressInfo.State ?? existingAddress.State;
            existingAddress.Country = request.AddressInfo.Country ?? existingAddress.Country;
            existingAddress.PostalCode =
                request.AddressInfo.PostalCode ?? existingAddress.PostalCode;
            existingAddress.Timestamp = DateTime.UtcNow;

            var updatedAddress = await _writer.UpdateAsync(existingAddress);
            return updatedAddress;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "/api/customers/{customerId}/address/{addressId}",
                    async (
                        IMediator mediator,
                        [FromRoute] int customerId,
                        [FromRoute] int addressId,
                        [FromBody] RequestDto data
                    ) =>
                    {
                        var response = await mediator.Send(
                            new Request(customerId, addressId, data)
                        );
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customer Address");
        }
    }
}
