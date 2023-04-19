using Assistant.Net.Storage.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage health check builder extensions.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a default storage health check registration.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="timeout">Health check timeout.</param>
    public static IHealthChecksBuilder AddStorage<TProviderHealthCheck>(this IHealthChecksBuilder builder, TimeSpan? timeout = null)
        where TProviderHealthCheck : class, IHealthCheck => builder
        .AddStorage<TProviderHealthCheck>(Microsoft.Extensions.Options.Options.DefaultName, timeout);

    /// <summary>
    ///     Adds a named storage health check registration.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="name">The name of the storage options instance.</param>
    /// <param name="timeout">Health check timeout.</param>
    public static IHealthChecksBuilder AddStorage<TProviderHealthCheck>(this IHealthChecksBuilder builder, string name, TimeSpan? timeout = null)
        where TProviderHealthCheck : class, IHealthCheck
    {
        builder.Services.Configure<HealthCheckServiceOptions>(options =>
        {
            var healthCheckName = HealthCheckNames.CreateName(name);
            var registration = options.Registrations.FirstOrDefault(x => x.Name == healthCheckName);
            if (registration != null)
                options.Registrations.Remove(registration);

            options.Registrations.Add(new(
                healthCheckName,
                factory: p => p.Create<TProviderHealthCheck>(name),
                failureStatus: HealthStatus.Unhealthy,
                tags: null,
                timeout));
        });
        return builder;
    }
}
