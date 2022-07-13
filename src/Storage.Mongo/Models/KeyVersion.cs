namespace Assistant.Net.Storage.Models;

/// <summary>
///     Unique historical/partitioned storage key representation.
/// </summary>
/// <param name="Key">Storage key.</param>
/// <param name="Version">Index number associated with <paramref name="Key"/>.</param>
public record KeyVersion(Key Key, long Version);
