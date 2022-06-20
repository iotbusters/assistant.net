using MongoDB.Bson.Serialization.Attributes;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     MongoDB storage key record.
/// </summary>
/// <param name="Key">Unique key identifier.</param>
/// <param name="Type">Key type name.</param>
/// <param name="Content">Binary key content.</param>
/// <param name="ValueType">Value type name.</param>
public record MongoKeyRecord(
    [property: BsonId] Key Key,
    string Type,
    byte[] Content,
    string ValueType);
