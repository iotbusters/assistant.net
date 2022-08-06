using Assistant.Net.HealthChecks;
using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

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
        .AddCheck<SqliteOptionsHealthCheck>(nameof(SqliteOptions), HealthStatus.Unhealthy, timeout: timeout);
}
