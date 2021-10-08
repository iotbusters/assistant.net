namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     The value uniquely representing a value across a partitioned storage.
    /// </summary>
    /// <param name="Id">Unique partition identifier.</param>
    /// <param name="Index">Unique index number in related partition.</param>
    public record PartitionKey(string Id, long Index);
}
