using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Core.Messaging;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqEventPublisher(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqEventPublisher> CreateAsync(string hostName = "localhost")
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "Auth.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        return new RabbitMqEventPublisher(connection, channel);
    }

    public async Task PublishAsync<T>(
        T @event,
        string topic,
        CancellationToken cancellationToken = default
    )
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "Auth.queue",
            mandatory: true,
            basicProperties: new BasicProperties { Persistent = true },
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
