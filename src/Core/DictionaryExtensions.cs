using System.Collections.Generic;

namespace Assistant.Net;

/// <summary>
/// 
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// 
    /// </summary>
    public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value) ? value : default;

    /// <summary>
    /// 
    /// </summary>
    public static TValue GetOrFail<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

    /// <summary>
    /// 
    /// </summary>
    public static bool SetUnlessDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? value)
    {
        if (Equals(value, default(TValue)))
            return false;

        dictionary[key] = value!;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool TryAddUnlessDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? value) =>
        !Equals(value, default(TValue)) && dictionary.TryAdd(key, value!);
}
