using Core.DataAccess;
using Core.Messaging.Abstractions;
using FluentValidation;
using MediatR;
using PhoneNumbers;
using Shared.Web.Helpers;
using Store.Core.Messeges;

namespace Store.Core.Features;

public sealed class CreateStore
{
    public class Response : Result<Models.Store>
    {
        public static implicit operator Response(Models.Store successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    };

    public sealed record DocumentRequestDto(string? DocumentType, string? FilePath);

    public sealed record Request(
        string StoreName,
        string StoreUrl,
        string? LogoPath,
        string OwnerName,
        string OwnerEmail,
        string OwnerPhoneNumber,
        string? ContactUsEmail,
        DocumentRequestDto[]? DocumentRequestDto
    ) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator(IEntityReader<Models.Store> _reader)
        {
            RuleFor(s => s.StoreName).NotEmpty().WithMessage("Store name must not be empty.");

            When(
                s => !string.IsNullOrEmpty(s.StoreName),
                () =>
                {
                    RuleFor(s => s.StoreName)
                        .Must(s => s.Any(c => !char.IsDigit(c)))
                        .WithMessage("Store name cannot consist of numbers only.");
                }
            );

            RuleFor(s => s.StoreUrl)
                .NotEmpty()
                .WithMessage("Store URL is required.")
                .Matches(@"^(https?://[A-Za-z0-9.-]+\.[A-Za-z]{2,}(\:\d+)?(/[\w/\-.]*)?)$")
                .WithMessage("Store URL format is invalid.")
                .MustAsync(
                    async (storeUrl, cancellationToken) =>
                    {
                        return !await _reader.ExistsAsync(s => s.StoreUrl == storeUrl);
                    }
                )
                .WithMessage("Store URL already exists.");

            RuleFor(r => r.OwnerPhoneNumber).NotEmpty().WithMessage("Phone Number is required");

            When(
                r => !string.IsNullOrEmpty(r.OwnerPhoneNumber),
                () =>
                {
                    RuleFor(r => r.OwnerPhoneNumber)
                        .Matches(@"^\+\d{1,3}\s\d+$")
                        .WithMessage("Invalid phone number format")
                        .Must(phoneNumber =>
                        {
                            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

                            try
                            {
                                PhoneNumber numberProto = phoneNumberUtil.Parse(phoneNumber, null);
                                return phoneNumberUtil.IsValidNumber(numberProto);
                            }
                            catch (NumberParseException)
                            {
                                return false;
                            }
                        })
                        .WithMessage("Phone Number not valid");
                }
            );

            RuleFor(r => r.OwnerName).NotEmpty().WithMessage("Store owner name is required");

            RuleFor(r => r.OwnerEmail).EmailAddress().WithMessage("Invalid email");

            When(
                r => !string.IsNullOrEmpty(r.OwnerPhoneNumber),
                () =>
                {
                    RuleFor(r => r.OwnerPhoneNumber)
                        .Matches(@"^\+\d{1,3}\s\d+$")
                        .WithMessage("invalid phone number")
                        .Must(phoneNumber =>
                        {
                            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

                            try
                            {
                                PhoneNumber numberProto = phoneNumberUtil.Parse(phoneNumber, null);
                                return phoneNumberUtil.IsValidNumber(numberProto);
                            }
                            catch (NumberParseException)
                            {
                                return false;
                            }
                        })
                        .WithMessage("invalid phone number");
                }
            );

            RuleFor(r => r.ContactUsEmail).EmailAddress().WithMessage("Invalid contact us email");
        }
    }

    public sealed class Handler(
        IEntityWriter<Models.Store> _writer,
        IEventPublisher _eventPublisher,
        Validator _validator,
        IMediator mediator
    ) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return new ValidationError(validationResult.Errors);
            }

            var store = await _writer.AddAsync(
                new Models.Store
                {
                    StoreName = request.StoreName,
                    StoreUrl = request.StoreUrl,
                    LogoPath = request.LogoPath,
                    OwnerName = request.OwnerName,
                    OwnerEmail = request.OwnerEmail,
                    OwnerPhoneNumber = request.OwnerPhoneNumber,
                    ContactUsEmail = request.ContactUsEmail,
                    Timestamp = DateTime.UtcNow,
                }
            );

            if (!string.IsNullOrEmpty(request.LogoPath))
            {
                var logoLocation = $"public/images/logo";
                await _eventPublisher.PublishAsync(
                    new FileMovedToPermanentStorageEvent(
                        store.LogoPath,
                        store.StoreId,
                        logoLocation,
                        DateTime.UtcNow
                    ),
                    "Store.FileMovedToPermanentStorageEvent",
                    cancellationToken
                );

                store.LogoPath = $"store-sys/{logoLocation}/{request.LogoPath}";
                await _writer.UpsertAsync(store);
            }

            foreach (var document in request.DocumentRequestDto ?? [])
            {
                var createStoreDocument = new CreateStoreDocument.Request(
                    store.StoreId,
                    new CreateStoreDocument.CreateStoreDocumentDto(
                        document.DocumentType,
                        document.FilePath
                    )
                );
                await mediator.Send(createStoreDocument);
            }

            await _eventPublisher.PublishAsync(
                new StoreCreatedEvent(
                    store.StoreId,
                    store.StoreUrl,
                    store.StoreName,
                    store.LogoPath,
                    store.OwnerName,
                    store.OwnerEmail,
                    store.OwnerPhoneNumber,
                    store.ContactUsEmail,
                    store.Timestamp
                ),
                "Store.StoreCreatedEvent",
                cancellationToken
            );

            return store;
        }
    }
}
