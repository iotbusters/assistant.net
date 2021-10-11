using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <inheritdoc cref="DeferredCachingInterceptor{TMessage,TResponse}"/>
    public sealed class DeferredCachingInterceptor : DeferredCachingInterceptor<IMessage<object>, object>, IMessageInterceptor
    {
        /// <summary/>
        public DeferredCachingInterceptor(IOptions<MessagingClientOptions> options) : base(options) { }
    }

    /// <summary>
    ///     Deferred message response (including failures) caching interceptor.
    /// </summary>
    public class DeferredCachingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private static readonly ConcurrentDictionary<string, DeferredCachingResult<TResponse>> deferredCache = new();

        private readonly MessagingClientOptions options;

        /// <summary/>
        public DeferredCachingInterceptor(IOptions<MessagingClientOptions> options) =>
            this.options = options.Value;

        /// <inheritdoc/>
        public Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
        {
            var key = message.GetSha1();
            return deferredCache.GetOrAdd(key, _ => next(message, token).WhenFaulted(ex =>
            {
                if (ex is MessageDeferredException || options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                    deferredCache.TryRemove(key, out var _);
            })).GetTask();
        }
    }
}
