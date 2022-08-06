using Assistant.Net.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.HealthChecks;

internal class MongoOptionsHealthCheck : IHealthCheck
{
    private readonly ILogger<MongoOptionsHealthCheck> logger;
    private readonly IOptionsMonitor<MongoOptions> monitor;
    private readonly string[] names;

    public MongoOptionsHealthCheck(
        ILogger<MongoOptionsHealthCheck> logger,
        IOptionsMonitor<MongoOptions> monitor,
        IEnumerable<IConfigureOptions<MongoOptions>> configureNamedOptions)
    {
        this.logger = logger;
        this.monitor = monitor;
        this.names = configureNamedOptions
            .OfType<ConfigureNamedOptions<MongoOptions>>()
            .Select(x => x.Name)
            .Distinct()
            .ToArray();
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token)
    {
        var tasks = names
            .Select(x => monitor.Get(x))
            .DistinctBy(x => (x.ConnectionString, x.DatabaseName))
            .Select(x => Check(x, token));
        var results = await Task.WhenAll(tasks);

        return results.Aggregate((x, y) => x && y)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }

    private async Task<bool> Check(MongoOptions options, CancellationToken token)
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
