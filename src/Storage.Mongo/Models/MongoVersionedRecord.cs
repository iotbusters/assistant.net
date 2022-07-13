using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     MongoDB historical/partitioned storage persisting record.
/// </summary>
public sealed class MongoVersionedRecord
{
    /// <summary>
    ///     Bson serialization only.
    /// </summary>
    public MongoVersionedRecord() { }

    /// <summary />
    public MongoVersionedRecord(
        KeyVersion key,
        string keyType,
        byte[] keyContent,
        byte[] valueContent,
        IDictionary<string, string> details)
    {
        Key = key;
        KeyType = keyType;
        KeyContent = keyContent;
        ValueContent = valueContent;
        Details = details;
    }

    /// <summary>
    ///     Unique key identifier.
    /// </summary>
    [BsonId]
    public KeyVersion Key { get; set; } = default!;

    /// <summary>
    ///     Key type name.
    /// </summary>
    public string KeyType { get; set; } = default!;

    /// <summary>
    ///     Binary key content.
    /// </summary>
    public byte[] KeyContent { get; set; } = default!;

    /// <summary>
    ///     <see cref="ValueContent"/> state version.
    /// </summary>
    public long Version { get; set; } = default!;

    /// <summary>
    ///     Binary value content.
    /// </summary>
    public byte[] ValueContent { get; set; } = default!;

    /// <summary>
    ///     Value content auditing details.
    /// </summary>
    public IDictionary<string, string> Details { get; set; } = default!;
}
