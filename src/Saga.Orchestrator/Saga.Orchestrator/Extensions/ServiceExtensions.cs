using System.Collections.Concurrent;
using System.Reflection;
using Contracts.Sagas.OrderManager;
using EventBus.Messages.IntegrationEvents.Events;
using EventBus.Messages.IntegrationEvents.Interfaces;
using Infrastructure.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Saga.Orchestrator.HttpRepository;
using Saga.Orchestrator.HttpRepository.Interfaces;
using Saga.Orchestrator.OrderManager;
using Saga.Orchestrator.Services;
using Saga.Orchestrator.Services.Interfaces;
using Serilog;
using Shared.Configurations;
using Shared.DTOs.Basket;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Saga.Orchestrator.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
        => services.AddTransient<ICheckoutSagaService, CheckoutSagaService>()
            .AddTransient<ISagaOrderManager<BasketCheckoutDto, OrderResponse >, SagaOrderManager>()
            .AddSingleton<IBusControl>(provider => provider.GetRequiredService<IBusControl>())
            .AddScoped<ISagaOrderManagerRabbitMq<BasketCheckoutDto, OrderResponse>, SagaOrderManagerRabbitMq>()
            .AddSingleton<ConcurrentDictionary<Guid, TaskCompletionSource<bool>>>(new ConcurrentDictionary<Guid, TaskCompletionSource<bool>>())
            // Đăng ký bus để tự động dừng khi ứng dụng dừng
            .AddHostedService<BusHostedService>()
        ;

    public static IServiceCollection ConfigureHttpRepository(this IServiceCollection services)
        => services.AddScoped<IOrderHttpRepository, OrderHttpRepository>()
            .AddScoped<IBasketHttpRepository, BasketHttpRepository>()
            .AddScoped<IInventoryHttpRepository, InventoryHttpRepository>()
        ;

    public static void ConfigureHttpClients(this IServiceCollection services)
    {
        ConfigureOrderHttpClient(services);
        ConfigureBasketHttpClient(services);
        ConfigureInventoryHttpClient(services);
    }

    private static void ConfigureOrderHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<IOrderHttpRepository, OrderHttpRepository>("OrdersAPI", (sp, cl) =>
        {
            cl.BaseAddress = new Uri("http://localhost:5005/api/v1/");
        });
        services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("OrdersAPI"));
    }
    
    private static void ConfigureBasketHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<IBasketHttpRepository, BasketHttpRepository>("BasketsAPI", (sp, cl) =>
        {
            cl.BaseAddress = new Uri("http://localhost:5004/api/");
        });
        services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("BasketsAPI"));
    }
    
    private static void ConfigureInventoryHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<IInventoryHttpRepository, InventoryHttpRepository>("InventoryAPI", (sp, cl) =>
        {
            cl.BaseAddress = new Uri("http://localhost:5006/api/");
        });
        services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("InventoryAPI"));
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
        
        // Thêm DbContext cho Saga
        services.AddDbContext<SagaDbContext>(options =>
            options.UseSqlServer(settings.SqlServerConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            }));
        services.AddMassTransit(config =>
        {
            // config.AddSagaStateMachine<OrderSaga, OrderSagaState>().InMemoryRepository();
            config.AddSagaStateMachine<OrderSaga, OrderSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.ExistingDbContext<SagaDbContext>();
                });
            config.AddConsumers(Assembly.GetExecutingAssembly());
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(mqConnection);
                cfg.ConfigureEndpoints(context);
                cfg.UseInMemoryOutbox();
                // cfg.ReceiveEndpoint("saga-completion", e =>
                // {
                //     e.Consumer(() => new SagaOrderManagerRabbitMq.SagaCompletedConsumer(
                //         context.GetService<Serilog.ILogger>(),
                //         context.GetService<ConcurrentDictionary<Guid, TaskCompletionSource<bool>>>()));
                // });
                // cfg.UseRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                // cfg.ReceiveEndpoint("order-saga", e =>
                // {
                //     e.ConfigureSaga<OrderSagaState>(context);
                // });
            });
        });
    }
}