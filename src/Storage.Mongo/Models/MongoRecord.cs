using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     MongoDB storage persisting record.
/// </summary>
public sealed class MongoRecord
{
    /// <summary>
    ///     Bson serialization only.
    /// </summary>
    public MongoRecord() { }

    /// <summary />
    public MongoRecord(
        string id,
        string keyType,
        byte[] keyContent,
        long version,
        string valueType,
        byte[] valueContent,
        IDictionary<string, string> details)
    {
        Id = id;
        KeyType = keyType;
        KeyContent = keyContent;
        Version = version;
        ValueType = valueType;
        ValueContent = valueContent;
        Details = details;
    }

    /// <summary>
    ///     Unique identifier.
    /// </summary>
    [BsonId]
    public string Id { get; set; } = default!;

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
    ///     Value type name.
    /// </summary>
    public string ValueType { get; set; } = default!;

    /// <summary>
    ///     Binary value content.
    /// </summary>
    public byte[] ValueContent { get; set; } = default!;

    /// <summary>
    ///     Value content auditing details.
    /// </summary>
    public IDictionary<string, string> Details { get; set; } = default!;
}