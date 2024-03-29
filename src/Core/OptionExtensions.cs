using Assistant.Net.Unions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Assistant.Net;

/// <summary>
///     Option usage facilitating extensions.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    ///     Converts a value wrapped in <see cref="Option{T}"/> from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>
    ///     with <paramref name="selector"/> function.
    /// </summary>
    [StackTraceHidden]
    public static async Task<Option<TResult>> MapOptionAsync<TSource, TResult>(this Option<TSource> source, Func<TSource, Task<TResult>> selector) => source switch
    {
        Some<TSource>(var value)    => Option.Some(await selector(value)),
        _                           => Option.None
    };

    /// <summary>
    ///     Converts a value wrapped in <see cref="Option{T}"/> from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>
    ///     with <paramref name="selector"/> function.
    /// </summary>
    [StackTraceHidden]
    public static Option<TResult> MapOption<TSource, TResult>(this Option<TSource> option, Func<TSource, TResult> selector) => option switch
    {
        Some<TSource>(var value)    => Option.Some(selector(value)),
        _                           => Option.None
    };

    /// <summary>
    ///     Filters a value wrapped in <see cref="Option{T}"/> against <paramref name="predicate"/> function.
    /// </summary>
    [StackTraceHidden]
    public static Option<TSource> WhereOption<TSource>(this Option<TSource> option, Func<TSource, bool> predicate) => option switch
    {
        Some<TSource>(var value)    => predicate(value) ? option : Option.None,
        _                           => Option.None
    };

    /// <summary>
    ///     Wraps nullable <typeparamref name="TSource"/> object into <see cref="Option{T}"/>.
    /// </summary>
    /// <returns>
    ///     <see cref="None{T}"/> if <typeparamref name="TSource"/> is null, otherwise <see cref="Some{T}"/>.
    /// </returns>
    public static Option<TSource> AsOption<TSource>(this TSource? option) => option switch
    {
        null  => Option.None,
        var x => Option.Some(x)
    };

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
    /// <exception cref="ArgumentException"/>
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
        }

        result = default;
        return false;
    }
}
