using Assistant.Net.Abstractions;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Options;

/// <summary>
///     Custom <see cref="IOptionsMonitorCache{TOptions}" /> to <see cref="IOptionsSnapshotCache{TOptions}"/> adapter.
/// </summary>
/// <typeparam name="TOptions">The options type being configured.</typeparam>
public sealed class OptionsSnapshotCache<TOptions> : OptionsCache<TOptions>, IOptionsSnapshotCache<TOptions> where TOptions : class { }
