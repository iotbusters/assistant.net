using System;
using System.Threading.Tasks;

namespace Assistant.Net
{
    public static class TaskExtensions
    {
        public static Task<TResult> Map<TSource, TResult>(
            this Task<TSource> source,
            Func<TSource, TResult> successSelector,
            Func<Exception, Exception> faultSelector)
        {
            return source.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    return faultSelector(t.Exception!.InnerException!).Throw<TResult>();
                if (t.IsCompletedSuccessfully)
                    return successSelector(t.Result);
                throw new TaskCanceledException();
            });
        }

        public static Task<TResult> MapSuccess<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> successSelector) => source
            .Map(successSelector, x => x);

        public static Task<TSource> MapFaulted<TSource>(this Task<TSource> source, Func<Exception, Exception> faultSelector) => source
            .Map(x => x, faultSelector);

        public static Task<TSource> When<TSource>(
            this Task<TSource> source,
            Action<TSource> successAction,
            Action<Exception> faultAction)
        {
            return source.ContinueWith<TSource>(t =>
            {
                if (t.IsFaulted)
                    faultAction(t.Exception!);
                if (t.IsCompletedSuccessfully)
                    successAction(t.Result);
                throw new TaskCanceledException();
            });
        }

        public static Task<TSource> WhenSuccess<TSource>(this Task<TSource> source, Action<TSource> successAction) => source
            .When(successAction, x => { });

        public static Task<TSource> WhenFaulted<TSource>(this Task<TSource> source, Action<Exception> faultAction) => source
            .When(x => { }, faultAction);
    }
}