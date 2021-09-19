using Assistant.Net.Storage.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Assistant.Net.Storage.Mongo.Models
{
    /// <summary>
    ///     MongoDB partitioned storage value record.
    /// </summary>
    /// <param name="Id">Unique identifier.</param>
    /// <param name="Type">Value type name.</param>
    /// <param name="Content">Binary value content.</param>
    /// <param name="Audit">Value content auditing details.</param>
    public record MongoPartitionValueRecord([property: BsonId] Guid Id, string Type, byte[] Content, Audit Audit);
}
