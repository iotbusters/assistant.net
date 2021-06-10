using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Assistant.Net
{
    public static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            await foreach (var item in source) yield return selector(item);
        }

        public static async IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            await foreach (var item in source)
                if(predicate(item))
                    yield return item;
        }

        public static async IAsyncEnumerable<TSource> AsAsync<TSource>(this IEnumerable<TSource> source)
        {
            foreach (var item in source)
                yield return await ValueTask.FromResult(item);
        }

        public static async Task<IEnumerable<TSource>> AsEnumerableAsync<TSource>(this IAsyncEnumerable<TSource> source)
        {
            var list = new List<TSource>();
            await using var enumerator = source.GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync())
                list.Add(enumerator.Current);

            return list.ToImmutableArray();
        }
    }
}