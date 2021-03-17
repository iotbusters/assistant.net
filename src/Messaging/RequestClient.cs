using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Assistant.Net.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Messaging
{
    public sealed class RequestClient : IRequestClient
    {
        /// <summary>
        ///     Important logging. It should be persisted. Temporary solution.
        /// </summary>
        private readonly ConcurrentDictionary<string, object> cache = new();
        private readonly IServiceProvider provider;

        public RequestClient(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            var interceptors = provider.GetServices<IInterceptor>()
                .OfType<IRequestInterceptor<TResponse>>()
                .Reverse()
                .Aggregate(
                    new Func<IRequest<TResponse>, Task<TResponse>>(InternalSend),
                    ChainInterceptors);
            return interceptors(request);
        }

        private static async Task<TResponse> InternalSend<TResponse>(IRequest<TResponse> request) =>
            default;// todo: implement

        private static Func<IRequest<TResponse>, Task<TResponse>> ChainInterceptors<TResponse>
            (Func<IRequest<TResponse>, Task<TResponse>> next, IRequestInterceptor<TResponse> current) =>
                x => current.Intercept(x, next);

        public Task Send(IRequest request) =>
            Evaluate(request);

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