using System.Net;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Common.Settings;

namespace Shop.Common.HealthCheck;

public static class Extensions
{
    public static IServiceCollection AddHealthCheck(this IServiceCollection services, string apiName)
    {
        var provider = services.BuildServiceProvider();
        var configuration = provider.GetService<IConfiguration>();
        var mongoDbConnection = configuration!.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>().ConnectionString;
        // Healcheck
        services.AddHealthChecks()
            .AddMongoDb(mongoDbConnection,
                        name: "mongodb",
                        timeout: TimeSpan.FromSeconds(5),
                        tags: new[] { "ready" });

        string endpoint = $"http://{Dns.GetHostName()}/healthchecks-{apiName}";

        services.AddHealthChecksUI(setup =>
        {
            setup.AddHealthCheckEndpoint(apiName, endpoint);
        }).AddInMemoryStorage();

        return services;
    }
    public static WebApplication UseHealthCheck(this WebApplication app, string apiName)
    {
        app.MapHealthChecks($"/healthchecks-{apiName}", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecksUI(config =>
        {
            config.UIPath = "/health-ui";
        });

        return app;
    }
}