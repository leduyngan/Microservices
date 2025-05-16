using Contracts.ScheduleJobs;
using Contracts.Services;
using Hangfire.API.Services;
using Hangfire.API.Services.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.Extensions;
using Infrastructure.ScheduledJobs;
using Infrastructure.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Configurations;

namespace Hangfire.API.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var hangFireSettings = configuration.GetSection(nameof(HangfireSettings))
            .Get<HangfireSettings>();
        services.AddSingleton(hangFireSettings);
        
        var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting))
            .Get<SMTPEmailSetting>();
        services.AddSingleton(emailSettings);

        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddTransient<IScheduleJobService, HangfireService>()
            .AddScoped(typeof(ISmtpEmailService), typeof(SmtpEmailService))
            .AddScoped<ISmtpEmailService, SmtpEmailService>()
            .AddTransient<IBackgroundJobService, BackgroundJobService>()
            ;
        services.ConfrugreHealthChecks();
        return services;
    } 
    
    private static void ConfrugreHealthChecks(this IServiceCollection services)
    {
        var databaseSettings = services.GetOptions<HangfireSettings>(nameof(HangfireSettings));
        services.AddHealthChecks()
            .AddMongoDb(databaseSettings.Storage.ConnectionString,
                name: "HangfireMongoDb Health",
                failureStatus: HealthStatus.Degraded);
    }
}