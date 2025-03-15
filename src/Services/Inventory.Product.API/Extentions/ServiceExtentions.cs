using Infrastructure.Extensions;
using MongoDB.Driver;

namespace Inventory.Product.API.Extentions;

public static class ServiceExtentions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection(nameof(DatabaseSettings))
            .Get<DatabaseSettings>();
        services.AddSingleton(databaseSettings);
        
        return services;
    }

    private static string GetMongoConnectionString(this IServiceCollection services)
    {
        var settings = services.GetOption<DatabaseSettings>(nameof(DatabaseSettings));
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
    }
}