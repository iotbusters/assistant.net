using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     SQLite storage persisting record.
/// </summary>
public class SqliteRecord
{
    /// <summary>
    ///     EF only.
    /// </summary>
    private SqliteRecord() { }

    /// <summary />
    public SqliteRecord(
        string keyId,
        string valueType,
        byte[] valueContent,
        long version,
        IDictionary<string, string> details)
    {
        KeyId = keyId;
        Version = version;
        ValueType = valueType;
        ValueContent = valueContent;
        Details = details.Select(x => new Detail(x.Key, x.Value)).ToArray();
    }

    /// <inheritdoc cref="SqliteKeyRecord"/>
    public SqliteKeyRecord Key { get; set; } = default!;

    /// <summary/>
    public string KeyId { get; set; } = default!;

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
    public IEnumerable<Detail> Details { get; set; } = default!;
}
