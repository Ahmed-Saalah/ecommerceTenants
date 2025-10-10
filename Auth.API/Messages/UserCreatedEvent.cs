namespace Auth.API.Messages;

public sealed record UserCreatedEvent(
    int UserId,
    int TenantId,
    string Username,
    string? Email,
    string? PhoneNumber,
    string DisplayName,
    string? Role
);
