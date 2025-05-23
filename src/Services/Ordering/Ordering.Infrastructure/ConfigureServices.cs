using Contracts.Domains.Interfaces;
using Contracts.Services;
using Infrastructure.Common.Interfaces;
using Infrastructure.Extensions;
using Infrastructure.Services;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Common.Interfaces;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repositories;
using Shared.Configurations;

namespace Ordering.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
        if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString))
            throw new ArgumentNullException("Connection string is not configured.");

        services.AddDbContext<OrderContext>(options =>
        {
            options.UseSqlServer(databaseSettings.ConnectionString,
                buider => buider.MigrationsAssembly(typeof(OrderContext).Assembly.FullName));
        });

        services.AddScoped<OrderContextSeed>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        services.AddScoped(typeof(ISmtpEmailService), typeof(SmtpEmailService));
        return services;
    }
}