using Core.DataAccess;
using FluentValidation;
using MediatR;
using Shared.Web.Helpers;

namespace Store.Core.Features;

public sealed class GetStoreById
{
    public sealed record Request(int StoreId) : IRequest<Response>;

    public sealed class Response : Result<Models.Store>
    {
        public static implicit operator Response(Models.Store store) => new() { Value = store };

        public static implicit operator Response(DomainError error) => new() { Error = error };
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.StoreId).GreaterThan(0).WithMessage("StoreId must be greater than 0.");
        }
    }

    public sealed class Handler(Validator validator, IEntityReader<Models.Store> reader)
        : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return new ValidationError(validationResult.Errors);
            }

            var store = await reader.GetByIdAsync(request.StoreId);

            return store is null
                ? new NotFound($"Store with ID {request.StoreId} was not found.")
                : store;
        }
    }
}
