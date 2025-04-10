using Infrastructure.Extensions;
using Inventory.Product.API.Entities;
using Inventory.Product.API.Services;
using Inventory.Product.API.Services.Interfaces;
using MongoDB.Driver;
using Shared.Configurations;

namespace Inventory.Product.API.Extentions;

public static class ServiceExtentions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoDbSetting = configuration.GetSection(nameof(MongoDbSettings))
            .Get<MongoDbSettings>();
        services.AddSingleton(mongoDbSetting);
        
        return services;
    }

    private static string GetMongoConnectionString(this IServiceCollection services)
    {
        var settings = services.GetOptions<MongoDbSettings>(nameof(MongoDbSettings));
        if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
        {
            throw new ArgumentException("The database settings are not configured correctly.");
        }
        
        var databaseName = settings.DatabaseName;
        var mongoConnectionString = settings.ConnectionString + "/" + databaseName + "?authSource=admin";
        return mongoConnectionString;
    }
    
    public static void ConfigureMongoDbClient(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(
            new MongoClient(GetMongoConnectionString(services)))
            .AddScoped(x => x.GetService<IMongoClient>()?.StartSession());
    }
    
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));
        services.AddScoped<IInventoryService, InventoryService>();
    }
}