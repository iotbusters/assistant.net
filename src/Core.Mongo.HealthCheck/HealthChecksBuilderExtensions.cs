using Assistant.Net.HealthChecks;
using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Assistant.Net;

/// <summary>
///     MongoDB health check builder extensions.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a <see cref="MongoOptions"/> configuration based health check implementation.
    /// </summary>
    public static IHealthChecksBuilder AddMongo(this IHealthChecksBuilder builder, TimeSpan? timeout = null) => builder
        .AddCheck<MongoOptionsHealthCheck>(nameof(MongoOptions), HealthStatus.Unhealthy, timeout: timeout);
}
