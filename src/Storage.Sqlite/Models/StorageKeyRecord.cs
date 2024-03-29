﻿namespace Assistant.Net.Storage.Models;

/// <summary>
///     SQLite storage key record.
/// </summary>
/// <param name="Id">Unique key identifier.</param>
/// <param name="Type">Key type name.</param>
/// <param name="Content">Binary key content.</param>
/// <param name="ValueType">Value type name.</param>
public record StorageKeyRecord(string Id, string Type, byte[] Content, string ValueType);
