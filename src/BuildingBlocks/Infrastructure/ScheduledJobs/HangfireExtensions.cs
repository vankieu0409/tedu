using Hangfire;
using Hangfire.PostgreSql;

using Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;

using Shared.Configurations;

namespace Infrastructure.ScheduledJobs;

public static class HangfireExtensions
{
    public static IServiceCollection AddTeduHangfireService(this IServiceCollection services)
    {
        var settings = services.GetOptions<HangFireSettings>("HangFireSettings");
        if (settings == null || settings.Storage == null ||
            string.IsNullOrEmpty(settings.Storage.ConnectionString))
            throw new Exception("HangFireSettings is not configured properly!");

        services.ConfigureHangfireServices(settings);
        services.AddHangfireServer(serverOptions
            =>
        { serverOptions.ServerName = settings.ServerName; });

        return services;
    }

    private static IServiceCollection ConfigureHangfireServices(this IServiceCollection services,
        HangFireSettings settings)
    {
        if (string.IsNullOrEmpty(settings.Storage.DBProvider))
            throw new Exception("HangFire DBProvider is not configured.");

        switch (settings.Storage.DBProvider.ToLower())
        {
            case "mongodb":
                //var mongoUrlBuilder = new MongoUrlBuilder(settings.Storage.ConnectionString);

                //var mongoClientSettings = MongoClientSettings.FromUrl(
                //    new MongoUrl(settings.Storage.ConnectionString));
                //mongoClientSettings.SslSettings = new SslSettings
                //{
                //    EnabledSslProtocols = SslProtocols.Tls12
                //};
                //var mongoClient = new MongoClient(mongoClientSettings);


                //var migrationOptions = new MongoMigrationOptions
                //{
                //    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                //    BackupStrategy = new CollectionMongoBackupStrategy()
                //};
                //services.AddHangfire((provider, config) =>
                //{
                //    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                //        .UseSimpleAssemblyNameTypeSerializer()
                //        .UseRecommendedSerializerSettings()
                //        .UseConsole(new ConsoleOptions())
                //        .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
                //        {
                //            MigrationOptions = migrationOptions,
                //            CheckConnection = true,
                //            Prefix = "SchedulerQueue",
                //            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
                //        });

                //    var jsonSettings = new JsonSerializerSettings
                //    {
                //        TypeNameHandling = TypeNameHandling.All
                //    };
                //    config.UseSerializerSettings(jsonSettings);
                //});
                //services.AddHangfireServer();
                //services.AddHangfireConsoleExtensions();
                break;
            case "postgresql":
                services.AddHangfire(x =>
                    x.UsePostgreSqlStorage(settings.Storage.ConnectionString));
                break;

            case "mssql":
                break;

            default:
                throw new Exception($"HangFire Storage Provider {settings.Storage.DBProvider} is not supported.");
        }
        return services;
    }
}