using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net;

/// <summary>
///     Async enumerable facilitating extensions.
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    ///     Projects each sequence element into new fort with the <paramref name="selector"/>.
    /// </summary>
    public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        await foreach (var item in source) yield return selector(item);
    }

    /// <summary>
    ///     Projects each sequence element into new fort with the <paramref name="selector"/>.
    /// </summary>
    public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
    {
        await foreach (var item in source) yield return await selector(item);
    }

    /// <summary>
    ///     Filters a sequence of values based on the <paramref name="predicate"/>.
    /// </summary>
    public static async IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        await foreach (var item in source)
            if(predicate(item))
                yield return item;
    }

    /// <summary>
    ///     Filters a sequence of values based on the <paramref name="predicate"/>.
    /// </summary>
    public static async IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
    {
        await foreach (var item in source)
            if (await predicate(item))
                yield return item;
    }

    /// <summary>
    ///     Returns the input typed as <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    public static async IAsyncEnumerable<TSource> AsAsync<TSource>(this IEnumerable<TSource> source)
    {
        foreach (var item in source)
            yield return await ValueTask.FromResult(item);
    }

    /// <summary>
    ///     Returns <paramref name="source" /> asynchronously broken into batches of <see cref="IEnumerable{T}"/>
    ///     by <paramref name="size"/>.
    /// </summary>
    /// <param name="source"/>
    /// <param name="size">Batch size.</param>
    /// <param name="token"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public static async IAsyncEnumerable<IEnumerable<TSource>> Batch<TSource>(this IAsyncEnumerable<TSource> source, int size, [EnumeratorCancellation]CancellationToken token = default)
    {
        if (size < 1)
            throw new IndexOutOfRangeException("Invalid batch size.");

        await using var enumerator = source.GetAsyncEnumerator(token);
        while (await enumerator.MoveNextAsync())
        {
            var list = new List<TSource>(size) {enumerator.Current};
            for (var i = 1; i < size && await enumerator.MoveNextAsync(); i++)
                list.Add(enumerator.Current);
            yield return list;
        }
    }

    /// <summary>
    ///     Returns the input typed as <see cref="IEnumerable{T}"/> asynchronously.
    /// </summary>
    public static async ValueTask<IEnumerable<TSource>> AsEnumerableAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken token = default) => await source
        .ToImmutableArrayAsync(token);

    /// <summary>
    ///     Returns the input typed as <see cref="ImmutableArray{T}"/> asynchronously.
    /// </summary>
    public static async ValueTask<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken token = default)
    {
        var list = new List<TSource>();
        await using var enumerator = source.GetAsyncEnumerator(token);

        while (await enumerator.MoveNextAsync())
            list.Add(enumerator.Current);

        return list.ToImmutableArray();
    }

    /// <summary>
    ///     Returns the input typed as <see cref="ImmutableList{T}"/> asynchronously.
    /// </summary>
    public static async ValueTask<ImmutableList<TSource>> ToImmutableListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken token = default)
    {
        var list = new List<TSource>();
        await using var enumerator = source.GetAsyncEnumerator(token);

        while (await enumerator.MoveNextAsync())
            list.Add(enumerator.Current);

        return list.ToImmutableList();
    }

    /// <summary>
    ///     Returns the input typed as an array asynchronously.
    /// </summary>
    public static async ValueTask<TSource[]> ToArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken token = default)
    {
        var list = new List<TSource>();
        await using var enumerator = source.GetAsyncEnumerator(token);

        while (await enumerator.MoveNextAsync())
            list.Add(enumerator.Current);

        return list.ToArray();
    }

    /// <summary>
    ///     Returns the input typed as <see cref="List{T}"/> asynchronously.
    /// </summary>
    public static async ValueTask<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken token = default)
    {
        var list = new List<TSource>();
        await using var enumerator = source.GetAsyncEnumerator(token);

        while (await enumerator.MoveNextAsync())
            list.Add(enumerator.Current);

        return list.ToList();
    }
}
