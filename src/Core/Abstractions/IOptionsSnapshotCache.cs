using Microsoft.Extensions.Options;

namespace Assistant.Net.Abstractions;

/// <summary>
///     Used by <see cref="IOptionsSnapshot{TOptions}"/> to cache <typeparamref name="TOptions"/> instances.
/// </summary>
/// <typeparam name="TOptions">The type of options being requested.</typeparam>
public interface IOptionsSnapshotCache<TOptions> : IOptionsMonitorCache<TOptions> where TOptions : class { }
