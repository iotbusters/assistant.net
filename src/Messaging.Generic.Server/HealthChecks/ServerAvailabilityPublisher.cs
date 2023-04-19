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
internal class ServerAvailabilityPublisher : IHealthCheckPublisher
{
    private readonly IOptionsMonitor<HealthCheckPublisherOptions> options;
    private readonly ServerAvailabilityService availabilityService;

    public ServerAvailabilityPublisher(
        IOptionsMonitor<HealthCheckPublisherOptions> options,
        ServerAvailabilityService availabilityService)
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
        var publisherOptions = options.CurrentValue;
        var expiration = publisherOptions.Timeout != Timeout.InfiniteTimeSpan
            ? publisherOptions.Period + publisherOptions.Timeout // period + timeout
            : publisherOptions.Period * 0.5; // period + 50%

        if (isHealthy)
            foreach (var name in names)
                await availabilityService.Register(name!, expiration, token);
        else
            foreach (var name in names)
                await availabilityService.Unregister(name!, token);
    }
}
