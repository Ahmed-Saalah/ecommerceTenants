using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public sealed class UpdateCustomer
{
    public sealed record Request(int CustomerId, RequestDto Data) : IRequest<Response>;

    public sealed record RequestDto(
        string? Username,
        string? Email,
        string? PhoneNumber,
        string? DisplayName,
        int? AddressId
    );

    public sealed class Response : Result<Customer>
    {
        public static implicit operator Response(Customer response) => new() { Value = response };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.CustomerId)
                .GreaterThan(0)
                .WithMessage("CustomerId must be greater than zero.");

            When(
                req => req.Data is not null,
                () =>
                {
                    RuleFor(r => r.Data.Username)
                        .NotEmpty()
                        .When(r => r.Data!.Username is not null)
                        .WithMessage("Username cannot be empty when provided.")
                        .MaximumLength(50)
                        .WithMessage("Username cannot exceed 50 characters.");

                    RuleFor(r => r.Data.Email)
                        .NotEmpty()
                        .When(r => r.Data!.Email is not null)
                        .WithMessage("Email cannot be empty when provided.")
                        .EmailAddress()
                        .WithMessage("Email format is invalid.")
                        .MaximumLength(100)
                        .WithMessage("Email cannot erceed 100 characters.");

                    RuleFor(r => r.Data.PhoneNumber)
                        .NotEmpty()
                        .When(r => r.Data!.PhoneNumber is not null)
                        .WithMessage("Phone number cannot be empty when provided.")
                        .MaximumLength(20)
                        .WithMessage("Phone number cannot erceed 20 characters.");

                    RuleFor(r => r.Data.DisplayName)
                        .NotEmpty()
                        .When(r => r.Data!.DisplayName is not null)
                        .WithMessage("Display name cannot be empty when provided.")
                        .MaximumLength(100)
                        .WithMessage("Display name cannot erceed 100 characters.");

                    RuleFor(r => r.Data.AddressId)
                        .GreaterThan(0)
                        .When(r => r.Data!.AddressId is not null)
                        .WithMessage("AddressId must be greater than zero when provided.");
                }
            );
        }
    }

    public sealed class Handler(
        IEntityReader<Customer> reader,
        IEntityWriter<Customer> writer,
        Validator validator
    ) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new ValidationError(validationResult.Errors);
            }

            var existingCustomer = await reader.GetByIdAsync(request.CustomerId);
            if (existingCustomer is null)
            {
                return new NotFoundError($"Customer with ID {request.CustomerId} not found.");
            }

            if (request.Data.Username is not null)
                existingCustomer.Username = request.Data.Username;

            if (request.Data.Email is not null)
                existingCustomer.Email = request.Data.Email;

            if (request.Data.PhoneNumber is not null)
                existingCustomer.PhoneNumber = request.Data.PhoneNumber;

            if (request.Data.DisplayName is not null)
                existingCustomer.DisplayName = request.Data.DisplayName;

            if (request.Data.AddressId is not null)
                existingCustomer.AddressId = request.Data.AddressId.Value;

            existingCustomer.Timestamp = DateTime.UtcNow;

            var updatedCustomer = await writer.UpdateAsync(existingCustomer);

            return updatedCustomer;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "/api/customers/{customerId}",
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
                .WithTags("Customers");
        }
    }
}
