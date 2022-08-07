using Assistant.Net.HealthChecks;
using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;

namespace Assistant.Net;

/// <summary>
///     SQLite health check builder extensions.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a <see cref="SqliteOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder AddSqlite(this IHealthChecksBuilder builder, TimeSpan? timeout = null) => builder
        .AddSqlite(nameof(SqliteOptions), timeout);

    /// <summary>
    ///     Adds a <see cref="SqliteOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder AddSqlite(this IHealthChecksBuilder builder, string name, TimeSpan? timeout = null) => builder
        .AddCheck<SqliteOptionsHealthCheck>(name, HealthStatus.Unhealthy, tags: null, timeout: timeout);

    /// <summary>
    ///     Replace a <see cref="SqliteOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder ReplaceSqlite(this IHealthChecksBuilder builder, TimeSpan? timeout = null) => builder
        .ReplaceSqlite(nameof(SqliteOptions), timeout);

    /// <summary>
    ///     Replace a <see cref="SqliteOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder ReplaceSqlite(this IHealthChecksBuilder builder, string name, TimeSpan? timeout = null)
    {
        builder.Services.Configure<HealthCheckServiceOptions>(options =>
        {
            var registration = options.Registrations.FirstOrDefault(x => x.Name == name);
            if (registration != null)
                options.Registrations.Remove(registration);

            options.Registrations.Add(new HealthCheckRegistration(
                name,
                factory: p => ActivatorUtilities.CreateInstance<SqliteOptionsHealthCheck>(p),
                failureStatus: HealthStatus.Unhealthy,
                tags: null,
                timeout));
        });
        return builder;
    }
}
