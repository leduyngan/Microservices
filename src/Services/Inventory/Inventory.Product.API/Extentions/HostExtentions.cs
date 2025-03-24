using Inventory.Product.API.Persistence;
using MongoDB.Driver;
using Shared.Configurations;

namespace Inventory.Product.API.Extentions;

public static class HostExtentions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var settings = services.GetService<MongoDbSettings>();
        if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
        {
            throw new ArgumentException("The database settings are not configured correctly.");
        }
        
        var mongoClient = services.GetRequiredService<IMongoClient>();
        new InventoryDbSeed()
            .SeedDataAsync(mongoClient, settings)
            .Wait();
        
        return host;
    }
}