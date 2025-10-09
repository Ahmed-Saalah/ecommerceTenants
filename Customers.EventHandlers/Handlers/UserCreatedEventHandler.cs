using System.Text.Json;
using Customers.EventHandlers.Clients.Customers;
using Customers.EventHandlers.Clients.Customers.Endpoints;
using Microsoft.Extensions.Logging;

namespace Customers.EventHandlers.Handlers;

public class UserCreatedEventHandler : IEventHandler
{
    private readonly ICustomersClient _customersClient;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        ICustomersClient customersClient,
        ILogger<UserCreatedEventHandler> logger
    )
    {
        _customersClient = customersClient;
        _logger = logger;
    }

    public bool CanHandle(string eventType) => eventType == nameof(UserCreatedEvent);

    public async Task HandleAsync(string json)
    {
        var user = JsonSerializer.Deserialize<UserCreatedEvent>(json);
        if (user is null)
            return;

        _logger.LogInformation("Handling UserCreatedEvent for {Email}", user.Email);

        if (user.Role == "Customer")
        {
            await _customersClient.ActAsync(
                new CreateCustomer.Endpoint(
                    new CreateCustomer.Request(
                        user.CustomerId,
                        user.TenantId,
                        user.Username,
                        user.Email,
                        user.PhoneNumber,
                        user.DisplayName
                    )
                )
            );
        }
    }

    public sealed record UserCreatedEvent(
        int CustomerId,
        int TenantId,
        string Username,
        string? Email,
        string? PhoneNumber,
        string DisplayName,
        string? Role
    );
}
