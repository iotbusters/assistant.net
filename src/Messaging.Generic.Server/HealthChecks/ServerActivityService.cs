using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     Activation managing service required to start and stop message handling.
/// </summary>
public sealed class ServerActivityService
{
    private readonly ILogger<ServerActivityService> logger;

    private CancellationTokenSource activityTokenSource = new(); // inactivated by default.

    /// <summary/>
    public ServerActivityService(ILogger<ServerActivityService> logger) =>
        this.logger = logger;

    /// <summary>
    ///     Determines if activation was requested.
    /// </summary>
    public bool IsActivationRequested => activityTokenSource.IsCancellationRequested;

    /// <summary>
    ///     Requests server activation.
    /// </summary>
    /// <remarks>
    ///     It ignores activated server.
    /// </remarks>
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

    /// <summary>
    ///     Requests server inactivation.
    /// </summary>
    /// <remarks>
    ///     It ignores inactivated server.
    /// </remarks>
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

    /// <summary>
    ///     Delays until server is activated.
    /// </summary>
    /// <remarks>
    ///     It ignores activated server.
    /// </remarks>
    public async Task DelayInactive(CancellationToken token)
    {
        var local = activityTokenSource;
        if (local.IsCancellationRequested)
            return;

        logger.LogWarning("The service is delaying until activated.");
        var watch = Stopwatch.StartNew();

        using var composedSource = CancellationTokenSource.CreateLinkedTokenSource(local.Token, token);
        await Task.WhenAny(Task.Delay(Timeout.Infinite, composedSource.Token));

        watch.Stop();
        logger.LogInformation("The service was delayed for {DelayTime}.", watch.Elapsed);
    }
}
