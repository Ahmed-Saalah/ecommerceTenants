using Core.DataAccess.Abstractions;
using Customers.Core.Models;
using FluentValidation;
using MediatR;
using Shared.Web.Helpers;

namespace Customers.Core.Features;

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
                return new NotFound($"address not found.");
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
}
