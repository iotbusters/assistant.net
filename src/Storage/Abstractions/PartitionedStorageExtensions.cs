using Assistant.Net.Unions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///     Partitioned storage extensions.
/// </summary>
public static class PartitionedStorageExtensions
{
    /// <summary>
    ///     Default time to delay used in <see cref="GetBlockingPartition{TKey,TValue}"/> method.
    /// </summary>
    public static readonly TimeSpan DefaultRetryIn = TimeSpan.FromSeconds(5);

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
