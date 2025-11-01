namespace Store.Core.Messeges;

public sealed record StoreCreatedEvent(
    int StoreId,
    string StoreName,
    string StoreUrl,
    string LogoPath,
    string OwnerName,
    string OwnerEmail,
    string OwnerPhoneNumber,
    string ContactUsEmail,
    DateTime Timestamp
);
