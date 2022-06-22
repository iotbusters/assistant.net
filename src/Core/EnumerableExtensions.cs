using System;
using System.Collections.Generic;

namespace Assistant.Net;

/// <summary>
///     IEnumerable extensions.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    ///     Returns <paramref name="source" /> broken into batches of <see cref="IEnumerable{T}"/> by <paramref name="size"/>.
    /// </summary>
    /// <param name="source"/>
    /// <param name="size">Batch size.</param>
    /// <exception cref="IndexOutOfRangeException"/>
    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
    {
        if (size < 1)
            throw new IndexOutOfRangeException("Invalid batch size.");

        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
            yield return enumerator.Batch(size);
    }

    private static IEnumerable<TSource> Batch<TSource>(this IEnumerator<TSource> enumerator, int size)
    {
        yield return enumerator.Current;
        for (var i = 1; i < size && enumerator.MoveNext(); i++)
            yield return enumerator.Current;
    }
}
