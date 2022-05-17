using System;
using System.Threading.Tasks;

namespace Assistant.Net;

/// <summary>
///     Task usage facilitating extensions.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    ///     Substitutes original exception or result depending on a task status when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static async Task<TResult> Map<TSource, TResult>(
        this Task<TSource> source,
        Func<TSource, TResult> completeSelector,
        Func<Exception, TResult> faultFactory)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (completeSelector == null) throw new ArgumentNullException(nameof(completeSelector));
        try
        {
            return completeSelector(await source);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return faultFactory(e);
        }
    }

    /// <summary>
    ///     Substitutes original exception or result depending on a task status when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static async Task<TResult> Map<TSource, TResult>(
        this Task<TSource> source,
        Func<TSource, TResult> completeSelector,
        Func<Exception, Exception>? faultSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (completeSelector == null) throw new ArgumentNullException(nameof(completeSelector));
        try
        {
            return completeSelector(await source);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            if (faultSelector == null)
                throw;
            return faultSelector(e).Throw<TResult>();
        }
    }

    /// <summary>
    ///     Substitutes original result with <paramref name="completeSelector"/> function when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static Task<TResult> MapCompleted<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> completeSelector) => source
        .Map(completeSelector, faultSelector: null);

    /// <summary>
    ///     Substitutes original exception with <paramref name="faultSelector"/> function when <paramref name="source"/> task faulted.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static Task<TSource> MapFaulted<TSource>(this Task<TSource> source, Func<Exception, Exception> faultSelector) => source
        .Map(completeSelector: x => x, faultSelector);

    /// <summary>
    ///     Substitutes original result with <paramref name="completeSelector"/> function when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static async Task<TResult> MapCompleted<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> completeSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (completeSelector == null) throw new ArgumentNullException(nameof(completeSelector));

        return await await source.MapCompleted<TSource, Task<TResult>>(completeSelector);
    }

    /// <summary>
    ///     Substitutes original exception or generates new result depending on a task status when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static async Task<TResult> Map<TResult>(
        this Task source,
        Func<TResult> completeSelector,
        Func<Exception, Exception>? faultSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (completeSelector == null) throw new ArgumentNullException(nameof(completeSelector));
        try
        {
            await source;
            return completeSelector();
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            if (faultSelector == null)
                throw;
                
            return faultSelector(e).Throw<TResult>();
        }
    }

    /// <summary>
    ///     Generates new result with <paramref name="completeFactory"/> function when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static Task<TResult> MapCompleted<TResult>(this Task source, Func<TResult> completeFactory) => source
        .Map(completeFactory, faultSelector: null);

    /// <summary>
    ///     Generates new result with <paramref name="completeFactory"/> function when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static async Task<TResult> MapCompleted<TResult>(this Task source, Func<Task<TResult>> completeFactory)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (completeFactory == null) throw new ArgumentNullException(nameof(completeFactory));

        return await await source.MapCompleted<Task<TResult>>(completeFactory);
    }

    /// <summary>
    ///     Callbacks <paramref name="completeAction"/> or <paramref name="faultAction"/> depending on a task status
    ///     when <paramref name="source"/> task completed.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static async Task<TSource> When<TSource>(
        this Task<TSource> source,
        Action<TSource>? completeAction = null,
        Action<Exception>? faultAction = null)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        try
        {
            var result = await source;
            completeAction?.Invoke(result);
            return result;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            faultAction?.Invoke(e);
            throw;
        }
    }

    /// <summary>
    ///     Callbacks <paramref name="completeAction"/> when <paramref name="source"/> task completed successfully.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static Task<TSource> WhenComplete<TSource>(this Task<TSource> source, Action<TSource> completeAction) => source
        .When(completeAction, faultAction: null);

    /// <summary>
    ///     Callbacks <paramref name="faultAction"/> when <paramref name="source"/> task faulted.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static Task<TSource> WhenFaulted<TSource>(this Task<TSource> source, Action<Exception> faultAction) => source
        .When(completeAction: null, faultAction);
}