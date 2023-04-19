using Assistant.Net.Storage.HealthChecks;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     MongoDB health check builder extensions.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a default <see cref="MongoOptions"/> configuration based health check implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="timeout">Health check timeout.</param>
    public static IHealthChecksBuilder AddMongo(this IHealthChecksBuilder builder, TimeSpan? timeout = null) => builder
        .AddMongo(Microsoft.Extensions.Options.Options.DefaultName, timeout);

    /// <summary>
    ///     Adds a named <see cref="MongoOptions"/> configuration based health check implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="name">The name of the storage options instance.</param>
    /// <param name="timeout">Health check timeout.</param>
    public static IHealthChecksBuilder AddMongo(this IHealthChecksBuilder builder, string name, TimeSpan? timeout = null) => builder
        .AddStorage<MongoOptionsHealthCheck>(name, timeout: timeout);
}
