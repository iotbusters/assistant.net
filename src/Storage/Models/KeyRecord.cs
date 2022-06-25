using System;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     Internal key representation object.
/// </summary>
public sealed class KeyRecord
{
    /// <summary/>
    public KeyRecord() { }

    /// <summary/>
    public KeyRecord(string id, string type, byte[] content, string valueType)
    {
        Id = id;
        Type = type;
        Content = content;
        ValueType = valueType;
    }

    /// <summary>
    ///     Unique identifier across specific <see cref="Type"/> keys.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     Key type name.
    /// </summary>
    public string Type { get; init; } = null!;

    /// <summary>
    ///     Binary key content.
    /// </summary>
    public byte[] Content { get; init; } = null!;

    /// <summary>
    ///     Value type name.
    /// </summary>
    public string ValueType { get; init; } = null!;

    /// <inheritdoc cref="Equals(object?)"/>
    public bool Equals(KeyRecord key) =>
        string.Equals(Type, key.Type)
        && string.Equals(ValueType, key.ValueType)
        && string.Equals(Id, key.Id);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj?.GetType() == GetType() && Equals((KeyRecord)obj);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Type, ValueType, Id);
}
