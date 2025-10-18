using Core.DataAccess.Abstractions;
using Customers.API.Extensions;
using Customers.API.Helpers;
using Customers.API.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customers.API.Features;

public sealed class GetCustomerById
{
    public sealed record Request(int CustomerId) : IRequest<Response>;

    public sealed class Response : Result<Customer>
    {
        public static implicit operator Response(Customer response) => new() { Value = response };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.CustomerId).GreaterThan(0).WithMessage("Id must be greater than zero.");
        }
    }

    public sealed class Handler(IEntityReader<Customer> customerReader, Validator validator)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return new ValidationError(validationResult.Errors);
            }

            var customer = await customerReader.GetByIdAsync(request.CustomerId);

            if (customer is null)
            {
                return new NotFoundError($"Customer with ID {request.CustomerId} not found.");
            }

            return customer;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/customers/{customerId}",
                    async (IMediator mediator, [FromRoute] int customerId) =>
                    {
                        var response = await mediator.Send(new Request(customerId));
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Customers");
        }
    }
}
