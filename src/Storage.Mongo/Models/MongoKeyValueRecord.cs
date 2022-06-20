using MongoDB.Bson.Serialization.Attributes;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     MongoDB partitioned storage key-value reference record.
/// </summary>
/// <param name="KeyVersion">Unique key identifier.</param>
/// <param name="ValueId">Unique identifier of related <see cref="MongoValueRecord.Id"/>.</param>
public record MongoKeyValueRecord(
    [property: BsonId] KeyVersion KeyVersion,
    string ValueId);
