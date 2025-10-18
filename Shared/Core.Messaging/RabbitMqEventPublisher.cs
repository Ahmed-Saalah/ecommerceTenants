using System.Text;
using System.Text.Json;
using Core.Messaging.Abstractions;
using RabbitMQ.Client;

namespace Core.Messaging;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;

    private RabbitMqEventPublisher(IConnection connection, IChannel channel, string exchangeName)
    {
        _connection = connection;
        _channel = channel;
        _exchangeName = exchangeName;
    }

    public static async Task<RabbitMqEventPublisher> CreateAsync(
        string exchangeName,
        string hostName = "localhost"
    )
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Declare the Topic Exchange. This is idempotent (it only creates it if it doesn't exist)
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic, // Use a Topic exchange
            durable: true,
            autoDelete: false
        );

        return new RabbitMqEventPublisher(connection, channel, exchangeName);
    }

    // The 'topic' parameter is now correctly named 'routingKey'
    public async Task PublishAsync<T>(
        T @event,
        string routingKey, // This is the event name, e.g., "Auth.UserCreatedEvent"
        CancellationToken cancellationToken = default
    )
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        // We DO NOT declare a queue here. The consumers do that.

        // We publish to the EXCHANGE
        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey, // The "topic" of the event
            mandatory: true,
            basicProperties: new BasicProperties { Persistent = true },
            body: body
        );
        Console.WriteLine(
            $"Published to exchange '{_exchangeName}' with routing key '{routingKey}'"
        );
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
