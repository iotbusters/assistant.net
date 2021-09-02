using System;
using System.Threading.Tasks;

namespace Assistant.Net
{
    /// <summary>
    ///     Task usage facilitating extensions.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        ///     Substitutes original exception or result depending on a task status when <paramref name="source"/> task completed.
        /// </summary>
        [Obsolete("Counterproductive compared to async/await.")]
        public static Task<TResult> Map<TSource, TResult>(
            this Task<TSource> source,
            Func<TSource, TResult> successSelector,
            Func<Exception, Exception>? faultSelector = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (successSelector == null) throw new ArgumentNullException(nameof(successSelector));
            return source.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    return successSelector(t.Result);
                if (t.IsFaulted)
                    return faultSelector == null
                        ? t.Exception!.InnerException!.Throw<TResult>()
                        : faultSelector(t.Exception!.InnerException!).Throw<TResult>();
                throw new TaskCanceledException();
            });
        }

        /// <summary>
        ///     Substitutes original result with <paramref name="successSelector"/> function when <paramref name="source"/> task completed.
        /// </summary>
        [Obsolete("Counterproductive compared to async/await")]
        public static Task<TResult> MapSuccess<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> successSelector) => source
            .Map(successSelector, null);

        /// <summary>
        ///     Substitutes original exception with <paramref name="faultSelector"/> function when <paramref name="source"/> task faulted.
        /// </summary>
        [Obsolete("Counterproductive compared to async/await")]
        public static Task<TSource> MapFaulted<TSource>(this Task<TSource> source, Func<Exception, Exception> faultSelector) => source
            .Map(x => x, faultSelector);

        /// <summary>
        ///     Substitutes original exception or generates new result depending on a task status when <paramref name="source"/> task completed.
        /// </summary>
        [Obsolete("Counterproductive compared to async/await")]
        public static Task<TResult> Map<TResult>(
            this Task source,
            Func<TResult> successSelector,
            Func<Exception, Exception>? faultSelector = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (successSelector == null) throw new ArgumentNullException(nameof(successSelector));
            return source.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    return successSelector();
                if (t.IsFaulted)
                    return faultSelector == null
                        ? t.Exception!.InnerException!.Throw<TResult>()
                        : faultSelector(t.Exception!.InnerException!).Throw<TResult>();
                throw new TaskCanceledException();
            });
        }

        /// <summary>
        ///     Generates new result with <paramref name="successSelector"/> function when <paramref name="source"/> task completed.
        /// </summary>
        [Obsolete("Counterproductive compared to async/await")]
        public static Task<TResult> MapSuccess<TResult>(this Task source, Func<TResult> successSelector) => source
            .Map(successSelector, x => x);

        /// <summary>
        ///     Callbacks <paramref name="successAction"/> or <paramref name="faultAction"/> depending on a task status
        ///     when <paramref name="source"/> task completed.
        /// </summary>
        public static Task<TSource> When<TSource>(
            this Task<TSource> source,
            Action<TSource>? successAction = null,
            Action<Exception>? faultAction = null)
        {







            //todo









            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    successAction?.Invoke(t.Result);
                    return t.Result;
                }
                if (t.IsFaulted)
                {
                    faultAction?.Invoke(t.Exception!);
                    return t.Exception!.Throw<TSource>();
                }
                throw new TaskCanceledException();
            });
        }

        /// <summary>
        ///     Callbacks <paramref name="successAction"/> when <paramref name="source"/> task completed successfully.
        /// </summary>
        public static Task<TSource> WhenSuccess<TSource>(this Task<TSource> source, Action<TSource> successAction) => source
            .When(successAction, faultAction: null);

        /// <summary>
        ///     Callbacks <paramref name="faultAction"/> when <paramref name="source"/> task faulted.
        /// </summary>
        public static Task<TSource> WhenFaulted<TSource>(this Task<TSource> source, Action<Exception> faultAction) => source
            .When(successAction: null, faultAction);
    }
}