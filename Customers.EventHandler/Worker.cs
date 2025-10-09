using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Customers.EventHandler;

public sealed record UserCreatedEvent(
    int CustomerId,
    int TenantId,
    string Username,
    string? Email,
    string? PhoneNumber,
    string DisplayName,
    string? Role
);

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private IConnection _connection;
    private IChannel _channel;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare the exchange and queue to ensure they exist
            await _channel.ExchangeDeclareAsync(
                exchange: "Auth.UserCreatedEvent",
                type: ExchangeType.Fanout,
                durable: true
            );
            await _channel.QueueDeclareAsync(
                queue: "Auth.UserCreatedEventQueue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            await _channel.QueueBindAsync(
                queue: "Auth.UserCreatedEventQueue",
                exchange: "Auth.UserCreatedEvent",
                routingKey: ""
            );

            _logger.LogInformation("Connected to RabbitMQ and declared queue.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

            _logger.LogInformation(
                "Received message for user: {UserId}",
                userCreatedEvent.CustomerId
            );

            try
            {
                // **Your business logic goes here**
                // For example, save the customer to the database
                _logger.LogInformation(
                    "Processing new customer with email: {Email}",
                    userCreatedEvent.Email
                );

                // Acknowledge the message to remove it from the queue
                _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing message for user: {UserId}",
                    userCreatedEvent.CustomerId
                );
                // Optionally, you can Nack the message to requeue it or send it to a dead-letter queue
                // _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "Auth.UserCreatedEventQueue",
            autoAck: false,
            consumer: consumer
        );
    }
}
