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

        private async Task<T> Evaluate<T>(IRequest<T> request)
        {
            var id = request.OperationId;
            if(cache.TryGetValue(id, out var cachedResponse))
                return (T)cachedResponse;

            var response = await request.Invoke();
            cache.TryAdd(id, response);
            return response;
        }

        private async Task Evaluate(IRequest request)
        {
            var id = request.OperationId;
            if(cache.TryGetValue(id, out var cachedResponse))
                if(cachedResponse == null)
                    return;
                else
                    ExceptionDispatchInfo.Capture((Exception)cachedResponse).Throw();

            var responseTask = request.Invoke();
            await Task.WhenAll(responseTask);

            cache.TryAdd(id, responseTask.Exception);
            await responseTask;
        }
    }
}