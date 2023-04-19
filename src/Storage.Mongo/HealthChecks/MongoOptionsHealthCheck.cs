using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.HealthChecks;

internal sealed class MongoOptionsHealthCheck : IHealthCheck
{
    private readonly ILogger<MongoOptionsHealthCheck> logger;
    private readonly MongoOptions options;

    public MongoOptionsHealthCheck(
        string name,
        ILogger<MongoOptionsHealthCheck> logger,
        IOptionsMonitor<MongoOptions> monitor)
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
        string? response;
        try
        {
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DatabaseName);
            var ping = await database.RunCommandAsync((Command<BsonDocument>)"{ping:1}", ReadPreference.Nearest, token);
            response = ping.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MongoDB database ping command has failed.");
            return false;
        }

        logger.LogDebug("MongoDB database ping command returned: {PingResponse}.", response);
        return response.Contains("ok");
    }
}
