using System.Collections.Generic;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     Sqlite partitioned storage value record.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Type">Value type name.</param>
/// <param name="Content">Binary value content.</param>
/// <param name="Details">Value content auditing details.</param>
public record PartitionedRecord(
    string Id,
    string Type,
    byte[] Content,
    IDictionary<string, object> Details) : IRecordIdentity;

internal interface IRecordIdentity
{
    string Id { get; }
}
