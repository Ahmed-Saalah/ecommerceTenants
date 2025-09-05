namespace Auth.API.Messages;

public sealed record UserCreatedEvent(
    int CustomerId,
    int TenantId,
    string Username,
    string? Email,
    string? PhoneNumber,
    string DisplayName,
    string? Role
);
