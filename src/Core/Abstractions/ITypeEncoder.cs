using System;

namespace Assistant.Net.Abstractions;

/// <summary>
///     Abstract type encoder responsible for encoding and decoding <see cref="Type"/>.
/// </summary>
public interface ITypeEncoder
{
    /// <summary>
    ///     Decode <paramref name="encodedType"/> value to <see cref="Type"/>.
    /// </summary>
    Type? Decode(string encodedType);

    /// <summary>
    ///     Encode <paramref name="type"/> to <see cref="string"/> value.
    /// </summary>
    string? Encode(Type type);
}
