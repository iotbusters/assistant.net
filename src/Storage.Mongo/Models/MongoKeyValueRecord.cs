using MongoDB.Bson.Serialization.Attributes;

namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     MongoDB partitioned storage key-value reference record.
    /// </summary>
    /// <param name="Key">Unique key identifier.</param>
    /// <param name="ValueId">Unique identifier of related <see cref="Assistant.Net.Storage.Models.MongoValueRecord.Id"/>.</param>
    public record MongoKeyValueRecord([property: BsonId] KeyVersion Key, string ValueId) : IRecordIdentity
    {
        /// <inheritdoc/>
        public string Id => Key.Id;
    }
}
