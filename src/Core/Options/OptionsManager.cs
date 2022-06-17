using Assistant.Net.Abstractions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Assistant.Net.Options;

/// <summary>
///     Reloadable and cacheable <see cref="IOptionsSnapshot{TOptions}"/> implementation.
/// </summary>
/// <typeparam name="TOptions">The options type being configured.</typeparam>
public sealed class OptionsManager<TOptions> : OptionsMonitor<TOptions>, IOptionsSnapshot<TOptions> where TOptions : class
{
    /// <summary/>
    public OptionsManager(
        IOptionsFactory<TOptions> factory,
        IEnumerable<IOptionsChangeTokenSource<TOptions>> sources,
        IOptionsSnapshotCache<TOptions> cache) : base(factory, sources, cache)
    {
    }

    /// <inheritdoc cref="IOptions{TOptions}.Value"/>
    public TOptions Value => CurrentValue;
}
