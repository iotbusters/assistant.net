using Assistant.Net.Abstractions;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Options;

/// <summary>
///     Options dependency configuration to reload <typeparamref name="TOptions"/>
///     on <typeparamref name="TOptionsDependency"/> change.
/// </summary>
/// <typeparam name="TOptions">The options type being dependent.</typeparam>
/// <typeparam name="TOptionsDependency">The options dependency type being tracked.</typeparam>
public sealed class ChangeOnConfigureNamedOptions<TOptions, TOptionsDependency> : IConfigureNamedOptions<TOptionsDependency>
    where TOptions : class
    where TOptionsDependency : class
{
    private readonly string optionName;
    private readonly string dependentOptionsName;
    private readonly IOptionsMonitorCache<TOptions> monitorCache;
    private readonly IOptionsSnapshotCache<TOptions> snapshotCache;

    /// <summary/>
    public ChangeOnConfigureNamedOptions(
        string optionName,
        string dependentOptionsName,
        IOptionsMonitorCache<TOptions> monitorCache,
        IOptionsSnapshotCache<TOptions> snapshotCache)
    {
        this.optionName = optionName;
        this.dependentOptionsName = dependentOptionsName;
        this.monitorCache = monitorCache;
        this.snapshotCache = snapshotCache;
    }

    /// <inheritdoc/>
    public void Configure(string name, TOptionsDependency options)
    {
        if (name != dependentOptionsName)
            return;

        monitorCache.TryRemove(optionName);
        snapshotCache.TryRemove(optionName);
    }

    /// <inheritdoc />
    public void Configure(TOptionsDependency options) { }
}
