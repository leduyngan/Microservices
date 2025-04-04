using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using Contracts.Domains.Interfaces;
using EventBus.Messages.IntegrationEvents.Interfaces;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Inventory.Grpc.Client;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Configurations;

namespace Basket.API.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
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
        
        return services;
    }
    
    public static IServiceCollection ConfigureServices(this IServiceCollection services) =>
        services.AddScoped<IBasketRepository, BasketRepository>()
            .AddTransient<ISerializeService, SerializeService>();

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
            config.UsingRabbitMq(( ctx , cfg ) =>
            {
                cfg.Host(mqConnection);
            });
            
            config.AddRequestClient<IBasketCheckOutEvent>();
        });
    }
}