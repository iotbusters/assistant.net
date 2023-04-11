using Assistant.Net.Storage.HealthChecks;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;

namespace Assistant.Net.Storage;

/// <summary>
///     MongoDB health check builder extensions.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a <see cref="MongoOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder AddMongo(this IHealthChecksBuilder builder, TimeSpan? timeout = null) => builder
        .AddMongo(nameof(MongoOptions), timeout);

    /// <summary>
    ///     Adds a <see cref="MongoOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder AddMongo(this IHealthChecksBuilder builder, string name, TimeSpan? timeout = null) => builder
        .AddCheck<MongoOptionsHealthCheck>(name, HealthStatus.Unhealthy, tags: null, timeout: timeout);

    /// <summary>
    ///     Replace a <see cref="MongoOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder ReplaceMongo(this IHealthChecksBuilder builder, TimeSpan? timeout = null) => builder
        .ReplaceMongo(nameof(MongoOptions), timeout);

    /// <summary>
    ///     Replace a <see cref="MongoOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder ReplaceMongo(this IHealthChecksBuilder builder, string name, TimeSpan? timeout = null)
    {
        builder.Services
            .TryAddSingleton<MongoOptionsHealthCheck>()
            .Configure<HealthCheckServiceOptions>(options =>
            {
                var registration = options.Registrations.FirstOrDefault(x => x.Name == name);
                if (registration != null)
                    options.Registrations.Remove(registration);

                options.Registrations.Add(new(
                    name,
                    factory: p => p.GetRequiredService<MongoOptionsHealthCheck>(),
                    failureStatus: HealthStatus.Unhealthy,
                    tags: null,
                    timeout));
            });
        return builder;
    }
}
