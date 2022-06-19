namespace Assistant.Net.Storage.Models;

/// <summary>
///     SQLite historical storage key record.
/// </summary>
/// <param name="Id">Unique key identifier.</param>
/// <param name="Type">Key type name.</param>
/// <param name="Content">Binary key content.</param>
/// <param name="ValueType">Value type content.</param>
public record HistoricalKeyRecord(string Id, string Type, byte[] Content, string ValueType);
