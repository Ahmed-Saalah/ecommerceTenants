using Core.DataAccess;
using Core.Messaging.Abstractions;
using FluentValidation;
using MediatR;
using Shared.Web.Helpers;
using Store.Core.Messeges;
using Store.Core.Models;

namespace Store.Core.Features;

public sealed class CreateStoreDocument
{
    public sealed class Response : Result<Document>
    {
        public static implicit operator Response(Document successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    }

    public sealed record CreateStoreDocumentDto(string DocumentType, string FilePath);

    public sealed record Request(int StoreId, CreateStoreDocumentDto StoreDocumentInfo)
        : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.StoreDocumentInfo.DocumentType)
                .NotEmpty()
                .WithMessage("Document type is required");

            RuleFor(r => r.StoreDocumentInfo.FilePath)
                .NotEmpty()
                .WithMessage("File path is required");

            RuleFor(r => r.StoreId).GreaterThan(0).WithMessage("Store Id is required");
        }
    }

    public sealed class Handler(
        IValidator<Request> validator,
        IEntityWriter<Document> writer,
        IEntityReader<Models.Store> reader,
        IEventPublisher eventPublisher
    ) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return new ValidationError(validationResult.Errors);
            }

            if (!await reader.ExistsAsync(s => s.StoreId == request.StoreId))
            {
                return new NotFound("Store not found");
            }

            var filesLocation = $"stores/documents/{request.StoreId}";
            var filePath = $"store-sys/{filesLocation}/{request.StoreDocumentInfo.FilePath}";

            var document = await writer.AddAsync(
                new Document
                {
                    StoreId = request.StoreId,
                    DocumentType = request.StoreDocumentInfo.DocumentType,
                    FilePath = filePath,
                    Timestamp = DateTime.UtcNow,
                }
            );

            await eventPublisher.PublishAsync(
                new FileMovedToPermanentStorageEvent(
                    request.StoreDocumentInfo.FilePath,
                    request.StoreId,
                    filesLocation,
                    DateTime.UtcNow
                ),
                "Store.FileMovedToPermanentStorageEvent",
                cancellationToken
            );

            return document;
        }
    }
}
