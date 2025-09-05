using Auth.API.DbContexts;
using Auth.API.Extensions;
using Auth.API.Models;
using Auth.API.Services;
using Core.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API;

public static class ServiceConfiguration
{
    public static async Task<IServiceCollection> ConfigureApplicationServiceAsync(
        this IServiceCollection svcs,
        IConfiguration config
    )
    {
        var dbConnectionString =
            config.GetValue<string>("DBConnection")
            ?? throw new Exception("DBConnection configuration not found");

        var sbConnectionString =
            config.GetValue<string>("SBConnection")
            ?? throw new Exception("SBConnection configuration not found");

        svcs.AddMediatR(_ => _.RegisterServicesFromAssemblyContaining<Program>());

        svcs.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(dbConnectionString);
        });

        svcs.AddHttpContextAccessor();

        svcs.AddLogging(_ => _.AddConsole().AddDebug());

        svcs.AddTransient<ILogger>(_ => _.GetRequiredService<ILogger<Program>>());

        svcs.AddDataAccess<AuthDbContext>();

        svcs.AddHttpContextAccessor();

        svcs.AddScoped<ITokenGenerator, TokenGenerator>();

        svcs.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        svcs.AddValidatorsFromAssembly(typeof(Program).Assembly);

        var rabbitHost = config.GetValue<string>("RabbitMQ:HostName") ?? "localhost";
        var publisher = await RabbitMqEventPublisher.CreateAsync(rabbitHost);
        svcs.AddSingleton<IEventPublisher>(publisher);

        return svcs;
    }
}
