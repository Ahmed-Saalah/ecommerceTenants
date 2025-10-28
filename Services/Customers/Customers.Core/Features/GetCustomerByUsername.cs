using Core.DataAccess;
using Customers.Core.Models;
using FluentValidation;
using MediatR;
using Shared.Web.Helpers;

namespace Customers.Core.Features;

public sealed class GetCustomerByUsername
{
    public sealed record Request(string Username) : IRequest<Response>;

    public sealed class Response : Result<Customer>
    {
        public static implicit operator Response(Customer response) => new() { Value = response };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Username).NotEmpty().WithMessage("Username is required");
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

            var customer = await customerReader.GetOneByAsync(c => c.Username == request.Username);

            if (customer is null)
            {
                return new NotFound($"Customer with username {request.Username} not found.");
            }

            return customer;
        }
    }
}
