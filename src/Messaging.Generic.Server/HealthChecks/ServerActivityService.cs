using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     Message handling activation mechanism implementation.
/// </summary>
internal sealed class ServerActivityService : IServerActivityService
{
    private readonly ILogger<ServerActivityService> logger;

    private CancellationTokenSource activityTokenSource = new(); // inactivated by default.

    /// <summary/>
    public ServerActivityService(ILogger<ServerActivityService> logger) =>
        this.logger = logger;

    /// <inheritdoc/>
    public bool IsActivationRequested => activityTokenSource.IsCancellationRequested;

    /// <inheritdoc/>
    public void Activate()
    {
        var local = activityTokenSource;
        if (local.IsCancellationRequested)
        {
            logger.LogDebug("The service is already activated.");
            return;
        }

        activityTokenSource.Cancel();
        logger.LogInformation("The service was activated.");
    }

    /// <inheritdoc/>
    public void Inactivate()
    {
        var local = activityTokenSource;
        if (!local.IsCancellationRequested)
        {
            logger.LogDebug("The service is already inactivated.");
            return;
        }

        Interlocked.CompareExchange(ref activityTokenSource, new(), local);
        logger.LogWarning("The service was inactivated.");
    }

    /// <inheritdoc/>
    public async Task DelayInactive(CancellationToken token)
    {
        var local = activityTokenSource;
        if (local.IsCancellationRequested)
            return;

        logger.LogWarning("The service is being delayed until activated.");
        var watch = Stopwatch.StartNew();

        using var composedSource = CancellationTokenSource.CreateLinkedTokenSource(local.Token, token);
        await Task.WhenAny(Task.Delay(Timeout.Infinite, composedSource.Token));

        watch.Stop();
        logger.LogInformation("The service was delayed for {DelayedTime}.", watch.Elapsed);
    }
}
