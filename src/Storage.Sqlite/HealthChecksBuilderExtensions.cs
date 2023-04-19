using Assistant.Net.Storage.HealthChecks;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     SQLite health check builder extensions.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a default <see cref="SqliteOptions"/> configuration based health check registration.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="timeout">Health check timeout.</param>
    public static IHealthChecksBuilder AddSqlite(this IHealthChecksBuilder builder, TimeSpan? timeout = null) => builder
        .AddSqlite(Microsoft.Extensions.Options.Options.DefaultName, timeout);

    /// <summary>
    ///     Adds a named <see cref="SqliteOptions"/> configuration based health check registration.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="name">The name of the storage options instance.</param>
    /// <param name="timeout">Health check timeout.</param>
    public static IHealthChecksBuilder AddSqlite(this IHealthChecksBuilder builder, string name, TimeSpan? timeout = null) => builder
        .AddStorage<SqliteOptionsHealthCheck>(name, timeout: timeout);
}
