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
    private readonly IOptions<HealthCheckPublisherOptions> publisherOptions;
    private readonly ServerAvailabilityService acceptanceService;

    public ServerAvailabilityPublisher(
        IOptions<HealthCheckPublisherOptions> publisherOptions,
        ServerAvailabilityService acceptanceService)
    {
        this.publisherOptions = publisherOptions;
        this.acceptanceService = acceptanceService;
    }

    public async Task PublishAsync(HealthReport report, CancellationToken token)
    {
        if (!report.Entries.Any())
            return;

        var isHealthy = report.Entries.All(x => x.Value.Status == HealthStatus.Healthy);
        var expiration = publisherOptions.Value.Timeout != Timeout.InfiniteTimeSpan
            ? publisherOptions.Value.Period + publisherOptions.Value.Timeout // period + timeout
            : publisherOptions.Value.Period * 0.5; // period + 50%

        if (isHealthy)
            await acceptanceService.Register(expiration, token);
        else
            await acceptanceService.Unregister(token);
    }
}
