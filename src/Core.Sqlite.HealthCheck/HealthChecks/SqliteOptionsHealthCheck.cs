using Assistant.Net.Options;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.HealthChecks;

internal sealed class SqliteOptionsHealthCheck : IHealthCheck
{
    private readonly ILogger<SqliteOptionsHealthCheck> logger;
    private readonly IOptionsMonitor<SqliteOptions> monitor;
    private readonly string[] names;

    public SqliteOptionsHealthCheck(
        ILogger<SqliteOptionsHealthCheck> logger,
        IOptionsMonitor<SqliteOptions> monitor,
        IEnumerable<IConfigureOptions<SqliteOptions>> configureNamedOptions)
    {
        this.logger = logger;
        this.monitor = monitor;
        this.names = configureNamedOptions
            .OfType<ConfigureNamedOptions<SqliteOptions>>()
            .Select(x => x.Name ?? Microsoft.Extensions.Options.Options.DefaultName)
            .Distinct()
            .ToArray();
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token)
    {
        var tasks = names
            .Select(x => monitor.Get(x))
            .DistinctBy(x => x.ConnectionString)
            .Select(x => Check(x, token));
        var results = await Task.WhenAll(tasks);

        return results.Aggregate((x, y) => x && y)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }

    private async Task<bool> Check(SqliteOptions options, CancellationToken token)
    {
        try
        {
            await using var connection = new SqliteConnection(options.ConnectionString);
            await connection.OpenAsync(token);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SQLite database ping command has failed.");
            return false;
        }

        logger.LogDebug("SQLite database ping command succeeded.");
        return true;
    }
}
