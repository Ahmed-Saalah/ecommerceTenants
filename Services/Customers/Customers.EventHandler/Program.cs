using Customers.EventHandler;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<UserCreatedEventHandler>();

var host = builder.Build();
host.Run();
