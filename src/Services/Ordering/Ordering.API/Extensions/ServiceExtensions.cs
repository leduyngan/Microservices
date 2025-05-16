using System.Reflection;
using EventBus.Messages.IntegrationEvents.Events;
using EventBus.Messages.IntegrationEvents.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.Extensions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ordering.API.Application.IntegrationEvents.EventHandler;
using Shared.Configurations;

namespace Ordering.API.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting))
            .Get<SMTPEmailSetting>();
        services.AddSingleton(emailSettings);
        
        var eventBusSettings = configuration.GetSection(nameof(EventBusSettings))
            .Get<EventBusSettings>();
        services.AddSingleton(eventBusSettings);
        
        var databaseSettings = configuration.GetSection(nameof(DatabaseSettings))
            .Get<DatabaseSettings>();
        services.AddSingleton(databaseSettings);
        services.ConfrugreHealthChecks();
        return services;
    }
    
    public static void ConfigureMassTransit(this IServiceCollection services)
    {
        var settings = services.GetOptions<EventBusSettings>("EventBusSettings");
        if (settings == null || string.IsNullOrEmpty(settings.HostAddress))
        {
            throw new ArgumentNullException("EventBusSetting is not configured.");
        }
        
        var mqConnection = new Uri(settings.HostAddress);
        services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
        services.AddMassTransit(config =>
        {
            //config.AddConsumers(typeof(Program).Assembly);
            config.AddConsumers(Assembly.GetExecutingAssembly());
            //config.AddConsumersFromNamespaceContaining<BasketCheckoutEventHandler>();
            config.UsingRabbitMq(( ctx , cfg ) =>
            {
                cfg.Host(mqConnection);
                // cfg.ReceiveEndpoint("basket-checkout-event", c =>
                // {
                //     c.ConfigureConsumer<BasketCheckOutConsumer>(ctx);
                // });
                
                // configure enpoints cho tất cả consumer của dự án
                cfg.ConfigureEndpoints(ctx);
            });
        });
    }
    
    private static void ConfrugreHealthChecks(this IServiceCollection services)
    {
        var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
        services.AddHealthChecks()
            .AddSqlServer(databaseSettings.ConnectionString,
                name: "SqlServer Health",
                failureStatus: HealthStatus.Degraded);
    }
}