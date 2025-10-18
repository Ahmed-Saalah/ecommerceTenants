using Core.Messaging;
using Core.Messaging.Extensions;
using Customers.EventHandler;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName)
);

var consumerConfig = builder.Configuration.GetSection("RabbitMq:Consumers:UserCreated");

builder.Services.AddRabbitMqEventConsumer<UserCreatedEvent, UserCreatedHandler>(
    exchangeName: consumerConfig["ExchangeName"]!,
    queueName: consumerConfig["QueueName"]!,
    routingKey: consumerConfig["RoutingKey"]!
);

var host = builder.Build();
host.Run();
