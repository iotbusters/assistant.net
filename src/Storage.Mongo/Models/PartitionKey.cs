namespace Assistant.Net.Storage.Mongo.Models
{
    /// <summary>
    ///     The value uniquely representing a value across a partitioned storage.
    /// </summary>
    public record PartitionKey(string Id, long Index);
}
