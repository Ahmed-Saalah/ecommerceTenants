using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using IConnection = RabbitMQ.Client.IConnection;

namespace Customers.EventHandler;

public class UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger) : BackgroundService
{
    public sealed record UserCreatedEvent(
        int UserId,
        string Username,
        string? Email,
        string? PhoneNumber,
        string DisplayName,
        string? Role
    );

    private readonly ILogger<UserCreatedEventHandler> _logger = logger;
    private IConnection? _connection;
    private IChannel? _channel;

    // Define your exchange and queue names
    private const string ExchangeName = "auth_exchange";
    private const string QueueName = "customers.user_created";
    private const string RoutingKey = "Auth.UserCreatedEvent";

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("Connected to RabbitMQ.");

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true
        );
        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey
        );

        _logger.LogInformation("Declared RabbitMQ topology.");

        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            _logger.LogError("RabbitMQ channel is not available. Handler cannot start.");
            return Task.CompletedTask;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);
                Console.WriteLine("Waaaasaaaal.");

                if (userCreatedEvent is not null)
                {
                    _logger.LogInformation(
                        "Processing user creation for ID: {UserId}, Email: {UserEmail}",
                        userCreatedEvent.UserId,
                        userCreatedEvent.Email
                    );
                    // --- YOUR BUSINESS LOGIC GOES HERE ---
                }

                // Acknowledge the message
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message.");
                await _channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: false
                );
            }
        };

        // --- Start Consuming ---
        _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);

        _logger.LogInformation(
            "Consumer started. Waiting for messages on queue '{QueueName}'...",
            QueueName
        );

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ event handler.");

        _channel?.CloseAsync(cancellationToken: cancellationToken);
        _connection?.CloseAsync(cancellationToken: cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
