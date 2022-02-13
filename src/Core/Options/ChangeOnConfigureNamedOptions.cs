using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Options;

/// <summary>
///     Options dependency configuration.
/// </summary>
/// <typeparam name="TOptions">The options type being dependent.</typeparam>
/// <typeparam name="TOptionsDependency">The options dependency type being tracked.</typeparam>
public class ChangeOnConfigureNamedOptions<TOptions, TOptionsDependency> : IConfigureNamedOptions<TOptionsDependency>
    where TOptions : class
    where TOptionsDependency : class
{
    private readonly Action<string> clearCache;

    /// <summary/>
    public ChangeOnConfigureNamedOptions(string optionName, string dependentOptionsName, IOptionsMonitorCache<TOptions> cache)
    {
        this.clearCache = name =>
        {
            if (name == dependentOptionsName)
                cache.TryRemove(optionName);
        };
    }

    /// <inheritdoc />
    public void Configure(string name, TOptionsDependency options) => clearCache(name);

    /// <inheritdoc />
    public void Configure(TOptionsDependency options) { }
}
