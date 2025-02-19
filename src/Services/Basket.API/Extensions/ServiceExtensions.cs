using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using Contracts.Common.Interfaces;
using Infrastructure.Common;

namespace Basket.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services) =>
        services.AddScoped<IBasketRepository, BasketRepository>()
            .AddTransient<ISerializeService, SerializeService>();

    public static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnnectionString = configuration.GetSection("CacheSettings:ConnectionString").Value;
        if (string.IsNullOrEmpty(redisConnnectionString))
        {
            throw new ArgumentNullException("Redis connection string is not configured.");
        }
        
        //Redis Configuration
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnnectionString);
    }
}