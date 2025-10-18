using Core.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqEventConsumer<TEvent, THandler>(
        this IServiceCollection services,
        string exchangeName,
        string queueName,
        string routingKey
    )
        where TEvent : class
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();
        services.AddScoped<THandler>();

        services.AddHostedService(sp => new RabbitMqEventConsumerService<TEvent, THandler>(
            sp.GetRequiredService<ILogger<RabbitMqEventConsumerService<TEvent, THandler>>>(),
            sp,
            exchangeName,
            queueName,
            routingKey
        ));

        return services;
    }
}
