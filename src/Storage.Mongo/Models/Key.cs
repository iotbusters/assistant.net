namespace Assistant.Net.Storage.Models;

/// <summary>
///     Unique storage key representation.
/// </summary>
/// <param name="Id">Unique key id within specific <paramref name="ValueType"/>.</param>
/// <param name="ValueType">Specific value type of a storage.</param>
public record Key(string Id, string ValueType);
