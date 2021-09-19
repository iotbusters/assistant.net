using Assistant.Net.Storage.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Assistant.Net.Storage.Mongo.Models
{
    /// <summary>
    ///     MongoDB partitioned storage key record.
    /// </summary>
    /// <param name="Key">Unique key identifier.</param>
    /// <param name="Type">Key type name.</param>
    /// <param name="Content">Binary key content.</param>
    public record MongoPartitionKeyRecord([property: BsonId] PartitionKey Key, string Type, byte[] Content, Audit Audit);
}
