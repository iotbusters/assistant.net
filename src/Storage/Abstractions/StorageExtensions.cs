using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public static async Task<TValue?> GetOrDefault<TKey, TValue>(
        this IStorage<TKey, TValue> storage,
        TKey key,
        CancellationToken token = default)
    {
        var option = await storage.TryGet(key, token);
        return option.GetValueOrDefault();
    }

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
    ///    Tries to add a detailed value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<StorageValue<TValue>> AddOrGet<TKey, TValue>(
        this IAdminStorage<TKey, TValue> storage,
        TKey key,
        Func<TKey, StorageValue<TValue>> addFactory,
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
    ///    Tries to add a detailed value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<StorageValue<TValue>> AddOrGet<TKey, TValue>(
        this IAdminStorage<TKey, TValue> storage,
        TKey key,
        StorageValue<TValue> value,
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
    public static Task<StorageValue<TValue>> AddOrUpdate<TKey, TValue>(
        this IAdminStorage<TKey, TValue> storage,
        TKey key,
        Func<TKey, StorageValue<TValue>> addFactory,
        Func<TKey, StorageValue<TValue>, StorageValue<TValue>> updateFactory,
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

    /// <summary>
    ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
    /// </summary>
    public static Task<StorageValue<TValue>> AddOrUpdate<TKey, TValue>(
        this IAdminStorage<TKey, TValue> storage,
        TKey key,
        StorageValue<TValue> value,
        CancellationToken token = default) =>
        storage.AddOrUpdate(
            key,
            _ => Task.FromResult(value),
            (_, _) => Task.FromResult(value),
            token);

    /// <summary>
    ///     Tries to find a value by associated to the <paramref name="key"/> or return a default value.
    /// </summary>
    public static async Task<TValue?> GetOrDefault<TKey, TValue>(
        this IHistoricalStorage<TKey, TValue> storage,
        TKey key,
        CancellationToken token = default)
    {
        var option = await storage.TryGet(key, token);
        return option.GetValueOrDefault();
    }

    /// <summary>
    ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<TValue> AddOrGet<TKey, TValue>(
        this IHistoricalStorage<TKey, TValue> storage,
        TKey key,
        Func<TKey, TValue> addFactory,
        CancellationToken token = default) =>
        storage.AddOrGet(key, x => Task.FromResult(addFactory(x)), token);

    /// <summary>
    ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<HistoricalValue<TValue>> AddOrGet<TKey, TValue>(
        this IHistoricalAdminStorage<TKey, TValue> storage,
        TKey key,
        Func<TKey, StorageValue<TValue>> addFactory,
        CancellationToken token = default) =>
        storage.AddOrGet(key, x => Task.FromResult(addFactory(x)), token);

    /// <summary>
    ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<TValue> AddOrGet<TKey, TValue>(
        this IHistoricalStorage<TKey, TValue> storage,
        TKey key,
        TValue value,
        CancellationToken token = default) =>
        storage.AddOrGet(key, _ => Task.FromResult(value), token);

    /// <summary>
    ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    public static Task<HistoricalValue<TValue>> AddOrGet<TKey, TValue>(
        this IHistoricalAdminStorage<TKey, TValue> storage,
        TKey key,
        StorageValue<TValue> value,
        CancellationToken token = default) =>
        storage.AddOrGet(key, _ => Task.FromResult(value), token);

    /// <summary>
    ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
    /// </summary>
    public static Task<TValue> AddOrUpdate<TKey, TValue>(
        this IHistoricalStorage<TKey, TValue> storage,
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
    public static Task<HistoricalValue<TValue>> AddOrUpdate<TKey, TValue>(
        this IHistoricalAdminStorage<TKey, TValue> storage,
        TKey key,
        Func<TKey, StorageValue<TValue>> addFactory,
        Func<TKey, StorageValue<TValue>, StorageValue<TValue>> updateFactory,
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
        this IHistoricalStorage<TKey, TValue> storage,
        TKey key,
        TValue value,
        CancellationToken token = default) =>
        storage.AddOrUpdate(
            key,
            _ => Task.FromResult(value),
            (_, _) => Task.FromResult(value),
            token);

    /// <summary>
    ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
    /// </summary>
    public static Task<HistoricalValue<TValue>> AddOrUpdate<TKey, TValue>(
        this IHistoricalAdminStorage<TKey, TValue> storage,
        TKey key,
        StorageValue<TValue> value,
        CancellationToken token = default) =>
        storage.AddOrUpdate(
            key,
            _ => Task.FromResult(value),
            (_, _) => Task.FromResult(value),
            token);

    /// <summary>
    ///     Default time to delay used in <see cref="GetBlockingPartition{TKey,TValue}"/> method.
    /// </summary>
    public static readonly TimeSpan DefaultRetryIn = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Tries to find a value by partition associated to the <paramref name="key"/> or return a default value.
    /// </summary>
    /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
    /// <typeparam name="TValue">A value object type is stored.</typeparam>
    /// <param name="storage">Original partitioned storage.</param>
    /// <param name="key">A key object.</param>
    /// <param name="index">A partition index.</param>
    /// <param name="token"/>
    /// <returns>An existed value or default.</returns>
    public static async Task<TValue?> GetOrDefault<TKey, TValue>(
        this IPartitionedStorage<TKey, TValue> storage,
        TKey key,
        long index,
        CancellationToken token = default)
    {
        var option = await storage.TryGet(key, index, token);
        return option.GetValueOrDefault();
    }

    /// <summary>
    ///     Gets values from partition associated to the <paramref name="key"/>
    ///     starting from <paramref name="startIndex"/> in none-blocking manner.
    /// </summary>
    /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
    /// <typeparam name="TValue">A value object type is stored.</typeparam>
    /// <param name="storage">Original partitioned storage.</param>
    /// <param name="key">A key object.</param>
    /// <param name="startIndex">A partition index.</param>
    /// <param name="token"/>
    /// <returns>A sequence of values in a partition.</returns>
    public static async IAsyncEnumerable<TValue> GetPartition<TKey, TValue>(
        this IPartitionedStorage<TKey, TValue> storage,
        TKey key,
        long startIndex = 1,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        if (startIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Expected value to be greater or equal than one.");

        var index = startIndex;
        while (await storage.TryGet(key, index++, token) is Some<TValue>(var value))
            yield return value;
    }

    /// <summary>
    ///     Gets values from partition associated to the <paramref name="key"/>
    ///     starting from <paramref name="startIndex"/> in blocking manner.
    /// </summary>
    /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
    /// <typeparam name="TValue">A value object type is stored.</typeparam>
    /// <param name="storage">Original partitioned storage.</param>
    /// <param name="key">A key object.</param>
    /// <param name="startIndex">A partition index.</param>
    /// <param name="pollIn">Time to delay between partition polling.</param>
    /// <param name="token"/>
    /// <returns>A sequence of values in a partition.</returns>
    public static async IAsyncEnumerable<TValue> GetBlockingPartition<TKey, TValue>(
        this IPartitionedStorage<TKey, TValue> storage,
        TKey key,
        long startIndex = 1,
        TimeSpan? pollIn = null,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var index = startIndex;
        while (!token.IsCancellationRequested)
        {
            await foreach (var value in storage.GetPartition(key, index, token))
            {
                yield return value;
                index++;
            }

            await Task.Delay(pollIn ?? DefaultRetryIn, token);
        }
    }
}
