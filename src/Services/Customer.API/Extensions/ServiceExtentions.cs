using Contracts.Domains.Interfaces;
using Customer.API.Persistence;
using Customer.API.Repositories.Interfaces;
using Customer.API.Services.Interfaces;
using Infrastructure.Common;
using Infrastructure.Common.Interfaces;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Shared.Configurations;

namespace Customer.API.Extensions;

public static class ServiceExtentions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection(nameof(DatabaseSettings))
            .Get<DatabaseSettings>();
        services.AddSingleton(databaseSettings);

        var hangFireSettings = configuration.GetSection(nameof(HangfireSettings))
            .Get<HangfireSettings>();
        services.AddSingleton(hangFireSettings);

        return services;
    }

    public static void ConfigureCustomerContext(this IServiceCollection services)
    {
        var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
        if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString))
            throw new ArgumentNullException("Connection string is not configured.");

        services.AddDbContext<CustomerContext>(options => options.UseNpgsql(databaseSettings.ConnectionString));
    }

    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<,,>), typeof(RepositoryBase<,,>))
            .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
            .AddScoped<ICustomerRepository, CustomerRepository>()
            .AddScoped<ICustomerService, CustomerService>();
    }
}