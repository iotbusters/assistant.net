using Assistant.Net.Storage.Options;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.HealthChecks;

internal sealed class SqliteOptionsHealthCheck : IHealthCheck
{
    private readonly ILogger<SqliteOptionsHealthCheck> logger;
    private readonly SqliteOptions options;

    public SqliteOptionsHealthCheck(
        string name,
        ILogger<SqliteOptionsHealthCheck> logger,
        IOptionsMonitor<SqliteOptions> monitor)
    {
        this.logger = logger;
        this.options = monitor.Get(name);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token) =>
        await Check(token)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();

    private async Task<bool> Check(CancellationToken token)
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
