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
                    return faultSelector(t.Exception!).Throw<TResult>();
                if (t.IsCompletedSuccessfully)
                    return successSelector(t.Result);
                var _ = t.Result; // fail if not success
                return default!; // unreachable
            });
        }

        public static Task<TResult> Map<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> successSelector) => source
            .Map(successSelector, x => x);

        public static Task<TSource> MapFaulted<TSource>(this Task<TSource> source, Func<Exception, Exception> faultSelector) => source
            .Map(x => x, faultSelector);

        public static Task<TSource> Pipe<TSource>(
            this Task<TSource> source,
            Action<TSource> successAction,
            Action<Exception> faultAction)
        {
            return source.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    faultAction(t.Exception!);
                if (t.IsCompletedSuccessfully)
                    successAction(t.Result);
                return t.Result; // fail if not success
            });
        }

        public static Task<TSource> Pipe<TSource>(this Task<TSource> source, Action<TSource> successAction) => source
            .Pipe(successAction, x => { });

        public static Task<TSource> PipeFaulted<TSource>(this Task<TSource> source, Action<Exception> faultAction) => source
            .Pipe(x => { }, faultAction);
    }
}