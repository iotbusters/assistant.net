namespace Assistant.Net.Storage.Models;

/// <summary>
///     The mapping between a specific <paramref name="Key"/> and an associated value version.
/// </summary>
/// <param name="Key">Unique key identifier.</param>
/// <param name="Version">Unique index number in related partition.</param>
public record KeyVersion(Key Key, long Version);
