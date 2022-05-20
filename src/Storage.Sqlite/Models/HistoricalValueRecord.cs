using System.Collections.Generic;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     SQLite historical storage persisting record.
/// </summary>
/// <param name="KeyId"><see cref="HistoricalKeyRecord"/> foreign key.</param>
/// <param name="ValueType">Value type name.</param>
/// <param name="ValueContent">Binary value content.</param>
/// <param name="Version"><see cref="ValueContent"/> state version.</param>
public record HistoricalValueRecord(
    string KeyId,
    string ValueType,
    byte[] ValueContent,
    long Version)
{
    /// <summary/>
    public HistoricalValueRecord(string keyId, string valueType, byte[] valueContent, long version, IEnumerable<StorageValueDetail> details)
    :this(keyId,valueType,valueContent, version)
    {
        Details = details;
    }

    /// <summary>
    ///     Value content auditing details.
    /// </summary>
    public IEnumerable<StorageValueDetail> Details { get; } = null!;
}
