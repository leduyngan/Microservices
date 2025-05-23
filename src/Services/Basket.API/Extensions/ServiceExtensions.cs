using System.Reflection;
using Basket.API.GrpcServices;
using Basket.API.IntegrationEvents;
using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using Basket.API.Services;
using Basket.API.Services.Interfaces;
using Common.Logging;
using Contracts.Domains.Interfaces;
using EventBus.Messages.IntegrationEvents.Interfaces;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Infrastructure.Policies;
using Inventory.Grpc.Client;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Configurations;

namespace Basket.API.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var eventBusSettings = configuration.GetSection(nameof(EventBusSettings))
            .Get<EventBusSettings>();
        services.AddSingleton(eventBusSettings);

        var cacheSettings = configuration.GetSection(nameof(CacheSettings))
            .Get<CacheSettings>();
        services.AddSingleton(cacheSettings);

        var grpcSettings = configuration.GetSection(nameof(GrpcSettings))
            .Get<GrpcSettings>();
        services.AddSingleton(grpcSettings);

        var backgroundJobSettings = configuration.GetSection(nameof(BackgroundJobSettings))
            .Get<BackgroundJobSettings>();
        services.AddSingleton(backgroundJobSettings);

        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IBasketRepository, BasketRepository>()
            .AddTransient<ISerializeService, SerializeService>()
            .AddTransient<IEmailTemplateService, BasketEmailTemplateService>()
            .AddTransient<LoggingDelegatingHandler>()
            ;
        services.ConfrugreHealthChecks();
        return services;
    }
       
    public static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = services.GetOptions<CacheSettings>("CacheSettings");
        if (string.IsNullOrEmpty(settings?.ConnectionString))
        {
            throw new ArgumentNullException("Redis connection string is not configured.");
        }

        //Redis Configuration
        services.AddStackExchangeRedisCache(options => options.Configuration = settings.ConnectionString);
    }

    public static IServiceCollection ConfigureGrpcServices(this IServiceCollection services)
    {
        var settings = services.GetOptions<GrpcSettings>(nameof(GrpcSettings));
        services.AddGrpcClient<StockProtoService.StockProtoServiceClient>(x => x.Address = new Uri(settings.StockUrl));
        services.AddScoped<StockItemGrpcService>();

        return services;
    }

    public static void ConfigureHttpClientService(this IServiceCollection services)
    {
        services.AddHttpClient<BackgroundJobHttpService>()
            //.AddHttpMessageHandler<LoggingDelegatingHandler>()
            .UseImmediateHttpRetryPolicy()
            .ConfigureTimeoutPolicy()
            //.UseCircuitBreakerPolicy()
            ;
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
            config.AddConsumers(Assembly.GetExecutingAssembly());
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(mqConnection);
                cfg.ConfigureEndpoints(context);
                // cfg.ReceiveEndpoint("get-basket", e => e.ConfigureConsumer<GetBasketConsumer>(context));
                // cfg.ReceiveEndpoint("delete-basket", e => e.ConfigureConsumer<DeleteBasketConsumer>(context));
            });

            config.AddRequestClient<IBasketCheckOutEvent>();
        });
    }

    private static void ConfrugreHealthChecks(this IServiceCollection services)
    {
        var databaseSettings = services.GetOptions<CacheSettings>("CacheSettings");
        services.AddHealthChecks()
            .AddRedis(databaseSettings.ConnectionString,
                name: "MySql Health",
                failureStatus: HealthStatus.Degraded);
    }
}