using Auth.API;
using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Routes;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfiguration(
    new ConfigurationBuilder().AddJsonFile("appsettings", true, true).Build()
);

IConfiguration config = builder.Configuration;

builder
    .Services.AddEndpointsApiExplorer()
    .AddSwaggerServices()
    .AddSwaggerGen(o => o.OperationFilter<SwaggerDefaultOperationFilter>())
    .ConfigureApplicationService(builder.Configuration);

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.UseRouting();

app.MapGet("/", () => Results.Ok(new { Now = DateTime.UtcNow }));
app.MapGet("/api/ping", () => Results.Ok(new { Now = DateTime.UtcNow }));

app.MapUsersApi();

app.Run();
