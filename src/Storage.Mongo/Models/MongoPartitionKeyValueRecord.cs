using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     MongoDB partitioned storage key-value reference record.
    /// </summary>
    /// <param name="Key">Unique key identifier.</param>
    /// <param name="ValueId">Unique identifier of related <see cref="Assistant.Net.Storage.Mongo.Models.MongoPartitionValueRecord.Id"/>.</param>
    public record MongoPartitionKeyValueRecord([property: BsonId] PartitionKey Key, Guid ValueId);
}
