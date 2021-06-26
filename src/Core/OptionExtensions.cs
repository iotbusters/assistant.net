using System;
using System.Threading.Tasks;
using Assistant.Net.Unions;

namespace Assistant.Net
{
    public static class OptionExtensions
    {
        /// <summary>
        ///     Converts a value wrapped in <see cref="Option{T}"/> from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>
        ///     with <paramref name="mapper"/> function.
        /// </summary>
        public static Task<Option<TResult>> Map<TSource, TResult>(this Task<Option<TSource>> source, Func<TSource, TResult> mapper) => source
            .MapSuccess(x => Map(x, mapper));

        /// <summary>
        ///     Converts a value wrapped in <see cref="Option{T}"/> from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>
        ///     with <paramref name="mapper"/> function.
        /// </summary>
        public static async Task<Option<TResult>> Map<TSource, TResult>(this Task<Option<TSource>> source, Func<TSource, Task<TResult>> mapper) => await source switch
        {
            Some<TSource>(var value)    => Option.Some(await mapper(value)),
            _                           => Option.None
        };

        /// <summary>
        ///     Converts a value wrapped in <see cref="Option{T}"/> from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>
        ///     with <paramref name="mapper"/> function.
        /// </summary>
        public static async Task<Option<TResult>> Map<TSource, TResult>(this Option<TSource> source, Func<TSource, Task<TResult>> mapper) => source switch
        {
            Some<TSource>(var value)    => Option.Some(await mapper(value)),
            _                           => Option.None
        };

        /// <summary>
        ///     Converts a value wrapped in <see cref="Option{T}"/> from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>
        ///     with <paramref name="mapper"/> function.
        /// </summary>
        public static Option<TResult> Map<TSource, TResult>(this Option<TSource> option, Func<TSource, TResult> mapper) => option switch
        {
            Some<TSource>(var value)    => Option.Some(mapper(value)),
            _                           => Option.None
        };

        /// <summary>
        ///     Gets a wrapped value from <see cref="Some{T}"/> or default if <see cref="None"/>.
        /// </summary>
        public static Task<TSource?> GetValueOrDefault<TSource>(this Task<Option<TSource>> source) => source.MapSuccess(GetValueOrDefault);

        /// <summary>
        ///     Gets a wrapped value from <see cref="Some{T}"/> or default if <see cref="None"/>.
        /// </summary>
        public static TSource? GetValueOrDefault<TSource>(this Option<TSource> option) => option switch
        {
            Some<TSource>(var value)    => value,
            _                           => default
        };

        /// <summary>
        ///     Gets a wrapped value from <see cref="Some{T}"/> or throws an exception if <see cref="None"/>.
        /// </summary>
        /// <exception cref="ArgumentException" />
        public static TSource GetValueOrFail<TSource>(this Option<TSource> option) => option switch
        {
            Some<TSource>(var value)    => value,
            _                           => throw new ArgumentException("Expected Some value only.")
        };

        /// <summary>
        ///     Gets a wrapped value from <see cref="Option{T}"/> into <paramref name="result"/>.
        /// </summary>
        /// <returns>Indication if <paramref name="option"/> is <see cref="Some{T}"/> or <see cref="None"/>.</returns>
        public static bool TryGetValue<T>(this Option<T> option, out T? result)
        {
            if (option is Some<T>(var value))
            {
                result = value;
                return true;
            };
            result = default;
            return false;
        }
    }
}