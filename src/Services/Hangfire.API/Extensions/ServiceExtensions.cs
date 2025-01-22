using Contracts.ScheduledJobs;
using Contracts.Services;

using Hangfire.API.Services;
using Hangfire.API.Services.Interfaces;

using Infrastructure.Configurations;
using Infrastructure.Extensions;
using Infrastructure.ScheduledJobs;
using Infrastructure.Services;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using MongoDB.Driver;

using Shared.Configurations;

namespace Hangfire.API.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var hangFireSettings = configuration.GetSection(nameof(HangFireSettings))
            .Get<HangFireSettings>();
        services.AddSingleton(hangFireSettings);

        var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting))
            .Get<SMTPEmailSetting>();
        services.AddSingleton(emailSettings);

        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
        => services.AddTransient<IScheduledJobService, HangfireService>()
            .AddScoped<ISmtpEmailService, SmtpEmailService>()
            .AddScoped<IBackgroundJobService, BackgroundJobService>()
        ;

    public static void ConfigureHealthChecks(this IServiceCollection services)
    {
        var databaseSettings = services.GetOptions<HangFireSettings>(nameof(HangFireSettings));
        if (databaseSettings is null)
        {

        }
        else
        {
            IMongoClient a = new MongoClient();
            services.AddHealthChecks()
              .AddMongoDb(c => a.GetDatabase(databaseSettings.Storage.ConnectionString),
                  "MongoDb Health",
                  HealthStatus.Degraded);
        }

    }
}