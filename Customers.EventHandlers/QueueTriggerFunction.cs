using System.Text;
using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Customers.EventHandlers;

public class QueueTriggerFunction
{
    private readonly IEnumerable<IEventHandler> _handlers;
    private readonly ILogger<QueueTriggerFunction> _logger;

    public QueueTriggerFunction(
        IEnumerable<IEventHandler> handlers,
        ILogger<QueueTriggerFunction> logger
    )
    {
        _handlers = handlers;
        _logger = logger;
    }

    [FunctionName("QueueTriggerFunction")]
    public async Task Run(
        [RabbitMQTrigger("events", ConnectionStringSetting = "RabbitMqConnection")] byte[] message
    )
    {
        var json = Encoding.UTF8.GetString(message);

        // Deserialize base event wrapper (with type info)
        var baseEvent = JsonSerializer.Deserialize<BaseEvent>(json);

        if (baseEvent is null)
        {
            _logger.LogWarning("Received invalid message: {Message}", json);
            return;
        }

        _logger.LogInformation("Received event: {Type}", baseEvent.Type);

        // Dispatch to matching handler
        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(baseEvent.Type))
            {
                await handler.HandleAsync(json);
            }
        }
    }
}

// a lightweight wrapper that contains type info
public record BaseEvent(string Type);
