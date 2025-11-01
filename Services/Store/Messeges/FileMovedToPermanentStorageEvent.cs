namespace Store.Core.Messeges;

public sealed record FileMovedToPermanentStorageEvent(
    string LogoPath,
    int StoreId,
    string LogoLocation,
    DateTime Timestamp
);
