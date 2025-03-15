using System.Reflection;
using EventBus.Messages.IntegrationEvents.Events;
using EventBus.Messages.IntegrationEvents.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.Extensions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        return services;
        
        var eventBusSettings = configuration.GetSection(nameof(EventBusSettings))
            .Get<EventBusSettings>();
        services.AddSingleton(eventBusSettings);
    }
    
    public static void ConfigureMassTransit(this IServiceCollection services)
    {
        var settings = services.GetOption<EventBusSettings>("EventBusSettings");
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
}