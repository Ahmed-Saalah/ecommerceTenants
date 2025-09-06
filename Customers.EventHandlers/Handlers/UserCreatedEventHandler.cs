using System.Text;
using System.Text.Json;
using Customers.EventHandlers.Clients.Customers;
using Customers.EventHandlers.Clients.Customers.Endpoints;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Customers.EventHandlers.Handlers;

public static class UserCreatedEventHandler
{
    [FunctionName("UserCreatedEventHandler")]
    public static async Task Run(
        [RabbitMQTrigger("Auth.UserCreatedEvent", ConnectionStringSetting = "localhost")]
            byte[] message,
        ICustomersClient customersApi,
        ILogger logger
    )
    {
        var json = Encoding.UTF8.GetString(message);
        var user = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        if (user != null)
        {
            logger.LogInformation("Received UserCreatedEvent: {Email}", user.Email);
            if (user.Role == "Customer")
            {
                await customersApi.ActAsync(
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
