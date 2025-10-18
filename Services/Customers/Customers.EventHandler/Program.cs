using Core.Messaging.Extensions;
using Customers.EventHandler;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqEventConsumer<UserCreatedEvent, UserCreatedHandler>(
    exchangeName: "auth_exchange",
    queueName: "customers.user_created",
    routingKey: "Auth.UserCreatedEvent"
);

var host = builder.Build();
host.Run();
