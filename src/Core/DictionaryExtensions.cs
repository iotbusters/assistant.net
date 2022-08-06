using System.Collections.Generic;

namespace Assistant.Net;

/// <summary>
/// 
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    ///     Gets value by key if defined, otherwise it returns default value.
    /// </summary>
    public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value) ? value : default;

    /// <summary>
    ///     Gets value by key if defined, otherwise it throws <see cref="KeyNotFoundException"/>.
    /// </summary>
    /// <exception cref="KeyNotFoundException"/>
    public static TValue GetOrFail<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

    /// <summary>
    ///     Tries to set non-default value, otherwise it returns false.
    /// </summary>
    public static bool SetUnlessDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? value)
    {
        if (Equals(value, default(TValue)))
            return false;

        dictionary[key] = value!;
        return true;
    }

    /// <summary>
    ///     Tries to add non-default value, otherwise it returns false.
    /// </summary>
    public static bool TryAddUnlessDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? value) =>
        !Equals(value, default(TValue)) && dictionary.TryAdd(key, value!);
}
