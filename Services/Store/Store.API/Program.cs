using Store.API;
using Store.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfiguration(
    new ConfigurationBuilder().AddJsonFile("appsettings", true, true).Build()
);

IConfiguration config = builder.Configuration;

builder
    .Services.AddEndpointsApiExplorer()
    .AddSwaggerGen(c =>
    {
        c.CustomSchemaIds(opts => opts.FullName?.Replace("+", "."));
    });

ServiceConfiguration.ConfigureApplicationService(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapGet("/", () => Results.Ok(new { Now = DateTime.UtcNow }));
app.MapGet("/api/ping", () => Results.Ok(new { Now = DateTime.UtcNow }));

EndpointExtensions.MapFeatureEndpoints(app);

app.Run();
