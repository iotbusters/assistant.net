using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     Generic messaging server activation manager based on the server latest health check status.
/// </summary>
internal sealed class ServerActivityPublisher : IHealthCheckPublisher
{
    private readonly IServerActivityService service;

    public ServerActivityPublisher(IServerActivityService service) =>
        this.service = service;

    public Task PublishAsync(HealthReport report, CancellationToken token)
    {
        if (!report.Entries.Any())
            return Task.CompletedTask;

        var isHealthy = report.Entries.All(x => x.Value.Status == HealthStatus.Healthy);
        if (isHealthy)
            service.Activate();
        else
            service.Inactivate();

        return Task.CompletedTask;
    }
}
