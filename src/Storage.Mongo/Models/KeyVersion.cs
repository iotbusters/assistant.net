namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     The mapping between a specific key <paramref name="Id"/> and an associated value version.
    /// </summary>
    /// <param name="Id">Unique key identifier.</param>
    /// <param name="Version">Unique index number in related partition.</param>
    public record KeyVersion(string Id, long Version);
}
