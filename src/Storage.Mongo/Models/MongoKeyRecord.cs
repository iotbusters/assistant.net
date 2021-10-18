using MongoDB.Bson.Serialization.Attributes;

namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     MongoDB storage key record.
    /// </summary>
    /// <param name="Id">Unique key identifier.</param>
    /// <param name="Type">Key type name.</param>
    /// <param name="Content">Binary key content.</param>
    public record MongoKeyRecord([property: BsonId] string Id, string Type, byte[] Content) : IRecordIdentity;
}
