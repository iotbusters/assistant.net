using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///     Storage extensions.
/// </summary>
public static class StorageExtensions
{
    /// <summary>
    ///     Tries to find a value by associated to the <paramref name="key"/> or return a default value.
    /// </summary>
    public static Task<TValue?> GetOrDefault<TKey, TValue>(
        this IStorage<TKey, TValue> storage,
        TKey key,
        CancellationToken token = default) =>
        storage.TryGet(key, token).GetValueOrDefault();

    /// <summary>
    ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<TValue> AddOrGet<TKey, TValue>(
        this IStorage<TKey, TValue> storage,
        TKey key,
        Func<TKey, TValue> addFactory,
        CancellationToken token = default) =>
        storage.AddOrGet(key, x => Task.FromResult(addFactory(x)), token);

    /// <summary>
    ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<TValue> AddOrGet<TKey, TValue>(
        this IStorage<TKey, TValue> storage,
        TKey key,
        TValue value,
        CancellationToken token = default) =>
        storage.AddOrGet(key, _ => Task.FromResult(value), token);

    /// <summary>
    ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
    /// </summary>
    public static Task<TValue> AddOrUpdate<TKey, TValue>(
        this IStorage<TKey, TValue> storage,
        TKey key,
        Func<TKey, TValue> addFactory,
        Func<TKey, TValue, TValue> updateFactory,
        CancellationToken token = default) =>
        storage.AddOrUpdate(
            key,
            k => Task.FromResult(addFactory(k)),
            (k, old) => Task.FromResult(updateFactory(k, old)),
            token);

    /// <summary>
    ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
    /// </summary>
    public static Task<TValue> AddOrUpdate<TKey, TValue>(
        this IStorage<TKey, TValue> storage,
        TKey key,
        TValue value,
        CancellationToken token = default) =>
        storage.AddOrUpdate(
            key,
            _ => Task.FromResult(value),
            (_, _) => Task.FromResult(value),
            token);
}
