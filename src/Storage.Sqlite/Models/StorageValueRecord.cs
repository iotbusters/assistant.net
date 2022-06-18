using System.Collections.Generic;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     SQLite storage persisting value record.
/// </summary>
public sealed class StorageValueRecord
{
    /// <summary>
    ///     EF only.
    /// </summary>
    private StorageValueRecord() { }

    /// <summary />
    public StorageValueRecord(
        string keyId,
        string valueType,
        byte[] valueContent,
        long version,
        IEnumerable<StorageValueDetail> details)
    {
        KeyId = keyId;
        Version = version;
        ValueType = valueType;
        ValueContent = valueContent;
        Details = details;
    }

    /// <summary/>
    public string KeyId { get; init; } = default!;

    /// <summary>
    ///     <see cref="ValueContent"/> state version.
    /// </summary>
    public long Version { get; set; } = default!;

    /// <summary>
    ///     Value type name.
    /// </summary>
    public string ValueType { get; init; } = default!;

    /// <summary>
    ///     Binary value content.
    /// </summary>
    public byte[] ValueContent { get; set; } = default!;

    /// <summary>
    ///     Value content auditing details.
    /// </summary>
    public IEnumerable<StorageValueDetail> Details { get; set; } = default!;
}
