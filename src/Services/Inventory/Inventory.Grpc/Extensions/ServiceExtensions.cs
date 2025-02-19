using Grpc.HealthCheck;

using Infrastructure.Extensions;

using Inventory.Grpc.Repositories;
using Inventory.Grpc.Repositories.Interfaces;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using MongoDB.Driver;

using Shared.Configurations;

namespace Inventory.Grpc.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection(nameof(MongoDbSettings))
            .Get<MongoDbSettings>();
        services.AddSingleton(databaseSettings);

        return services;
    }

    private static string getMongoConnectionString(this IServiceCollection services)
    {
        var settings = services.GetOptions<MongoDbSettings>(nameof(MongoDbSettings));
        if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
            throw new ArgumentNullException("DatabaseSettings is not configured");

        var databaseName = settings.DatabaseName;
        var mongodbConnectionString = settings.ConnectionString + "/" + databaseName + "?authSource=admin";
        return mongodbConnectionString;
    }

    public static void ConfigureMongoDbClient(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(
            new MongoClient(getMongoConnectionString(services)))
            .AddScoped(x => x.GetService<IMongoClient>()?.StartSession());
    }

    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IInventoryRepository, InventoryRepository>();
    }

    public static void ConfigureHealthChecks(this IServiceCollection services)
    {
        var databaseSettings = services.GetOptions<MongoDbSettings>(nameof(MongoDbSettings));
        services.AddSingleton<HealthServiceImpl>();
        services.AddHostedService<StatusService>();
        services.AddHealthChecks()
            .AddMongoDb(databaseSettings.ConnectionString,
                name: "Inventory MongoDb Health",
                failureStatus: HealthStatus.Degraded)
            .AddCheck("Inventory Grpc Health", () => HealthCheckResult.Healthy());
    }
}