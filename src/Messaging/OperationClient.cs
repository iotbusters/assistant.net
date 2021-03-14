using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Assistant.Net.Messaging;

namespace Messaging
{
    public class OperationClient : IOperationClient
    {
        /// <summary>
        ///     Important logging. It should be persisted. Temporary solution.
        /// </summary>
        private readonly ConcurrentDictionary<string, object> cache = new();

        public Promise<TResponse> Send<TResponse>(IRequest<TResponse> request) =>
            new(Evaluate(request));

        public Promise Send(IRequest request) =>
            new(Evaluate(request));

        private Task<T> Evaluate<T>(IRequest<T> request)
        {
            var id = request.OperationId;
            if (TryGetResponseOrException(id, out T response))
                return Task.FromResult(response);

            var responseTask = request.Invoke();

            // todo: check delayed caching approach
            return responseTask.ContinueWith(x =>
            {
                if (x.IsFaulted)
                    cache.TryAdd(id, x.Exception);
                else
                    cache.TryAdd(id, x.Result);
                return x.Result;
            });

            // todo: delete or uncomment
            //await Task.WhenAll(responseTask);
            // if (responseTask.IsFaulted)
            //     cache.TryAdd(id, responseTask.Exception);
            // var response = await responseTask;
            // cache.TryAdd(id, response);
            // return response;
        }

        private Task Evaluate(IRequest request)
        {
            var id = request.OperationId;
            TryGetResponseOrException(id, out object _);

            var responseTask = request.Invoke();

            // todo: check delayed caching approach
            return responseTask.ContinueWith(x =>
            {
                cache.TryAdd(id, x.Exception);
                return;
            });

            // todo: delete or uncomment
            // await Task.WhenAll(responseTask);
            // cache.TryAdd(id, responseTask.Exception);
            // await responseTask;
        }

        private bool TryGetResponseOrException<T>(string key, out T value)
        {
            if (!cache.TryGetValue(key, out var cachedValue))
            {
                value = default;
                return false;
            }

            if (cachedValue is Exception ex)
                ExceptionDispatchInfo.Capture(ex).Throw();

            value = (T)cachedValue;
            return true;
        }
    }
}