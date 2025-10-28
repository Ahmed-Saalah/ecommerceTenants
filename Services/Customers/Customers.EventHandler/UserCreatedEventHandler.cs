using Core.Messaging.Abstractions;
using MediatR;

namespace Customers.EventHandler;

public class UserCreatedHandler(ILogger<UserCreatedHandler> logger, IMediator _mediator)
    : IEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedHandler> _logger = logger;

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing user creation for ID: {UserId}, Email: {UserEmail}",
            @event.UserId,
            @event.Email
        );

        // --- YOUR PURE BUSINESS LOGIC ---

        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("Customer created for user {UserId}", @event.UserId);
    }
}

public sealed record UserCreatedEvent(
    int UserId,
    string Username,
    string? Email,
    string? PhoneNumber,
    string DisplayName,
    string? Role
);
