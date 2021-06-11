using System;
using System.Threading.Tasks;
using Assistant.Net.Unions;

namespace Assistant.Net
{
    public static class OptionExtensions
    {
        public static Task<Option<TResult>> Map<TSource, TResult>(this Task<Option<TSource>> source, Func<TSource, TResult> mapper) => TaskExtensions.MapSuccess(source, x => Map(x, mapper));

        public static Option<TResult> Map<TSource, TResult>(this Option<TSource> option, Func<TSource, TResult> mapper) => option switch
        {
            Some<TSource>(var value)    => Option.Some(mapper(value)),
            _                           => Option.None
        };

        public static Task<TSource?> GetValueOrDefault<TSource>(this Task<Option<TSource>> source) => TaskExtensions.MapSuccess(source, GetValueOrDefault);

        public static TSource? GetValueOrDefault<TSource>(this Option<TSource> option) => option switch
        {
            Some<TSource>(var value)    => value,
            _                           => default
        };

        public static TSource GetValueOrFail<TSource>(this Option<TSource> option) => option switch
        {
            Some<TSource>(var value)    => value,
            _                           => throw new ArgumentException("Expected Some value only.")
        };

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