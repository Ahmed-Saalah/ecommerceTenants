using System.Text;
using System.Text.Json;
using Core.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using IConnection = RabbitMQ.Client.IConnection;

namespace Core.Messaging;

public class RabbitMqEventConsumerService<TEvent, THandler> : BackgroundService
    where TEvent : class
    where THandler : IEventHandler<TEvent>
{
    private readonly ILogger<RabbitMqEventConsumerService<TEvent, THandler>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly string _routingKey;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventConsumerService(
        ILogger<RabbitMqEventConsumerService<TEvent, THandler>> logger,
        IServiceProvider serviceProvider,
        string exchangeName,
        string queueName,
        string routingKey
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _exchangeName = exchangeName;
        _queueName = queueName;
        _routingKey = routingKey;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true
        );
        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: _exchangeName,
            routingKey: _routingKey
        );

        _logger.LogInformation(
            "RabbitMQ Consumer Service for {Event} started.",
            typeof(TEvent).Name
        );
        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            _logger.LogError("RabbitMQ channel is not available.");
            return Task.CompletedTask;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();

            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<TEvent>(message);

                if (@event is not null)
                {
                    await handler.HandleAsync(@event, stoppingToken);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message for {Event}.", typeof(TEvent).Name);
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
        _logger.LogInformation(
            "Consumer started. Waiting for messages on queue '{QueueName}'...",
            _queueName
        );

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Stopping RabbitMQ Consumer Service for {Event}.",
            typeof(TEvent).Name
        );
        _channel?.CloseAsync(cancellationToken: cancellationToken);
        _connection?.CloseAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
