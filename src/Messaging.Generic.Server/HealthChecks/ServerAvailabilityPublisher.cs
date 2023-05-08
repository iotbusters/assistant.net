using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Storage.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     Generic messaging server availability publisher based on the server latest health check status.
/// </summary>
internal sealed class ServerAvailabilityPublisher : IHealthCheckPublisher
{
    private readonly IOptionsMonitor<HealthCheckPublisherOptions> options;
    private readonly IServerAvailabilityService availabilityService;

    public ServerAvailabilityPublisher(
        IOptionsMonitor<HealthCheckPublisherOptions> options,
        IServerAvailabilityService availabilityService)
    {
        this.options = options;
        this.availabilityService = availabilityService;
    }

    public async Task PublishAsync(HealthReport report, CancellationToken token)
    {
        if (!report.Entries.Any())
            return;

        var isHealthy = report.Entries.Values.All(x => x.Status == HealthStatus.Healthy);
        var names = report.Entries.Keys.Select(HealthCheckNames.GetOptionName).Where(x => x != null);
        var timeToLive = options.CurrentValue.Period * 1.2; // +20% to health check period.

        if (isHealthy)
            foreach (var name in names)
                await availabilityService.Register(name!, timeToLive, token);
        else
            foreach (var name in names)
                await availabilityService.Unregister(name!, token);
    }
}
