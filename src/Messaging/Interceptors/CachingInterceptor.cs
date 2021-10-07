using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <inheritdoc cref="CachingInterceptor{TMessage,TResponse}"/>
    public class CachingInterceptor : CachingInterceptor<IMessage<object>, object>, IMessageInterceptor
    {
        /// <summary/>
        public CachingInterceptor(IStorage<string, CachingResult> cache, IOptions<MessagingClientOptions> options) : base(cache, options) { }
    }

    /// <summary>
    ///     Message response (including failures) caching interceptor.
    /// </summary>
    /// <remarks>
    ///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/>
    /// </remarks>
    public class CachingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly IStorage<string, CachingResult> cache;
        private readonly IOptions<MessagingClientOptions> options;

        /// <summary/>
        public CachingInterceptor(IStorage<string, CachingResult> cache, IOptions<MessagingClientOptions> options)
        {
            this.cache = cache;
            this.options = options;
        }

        /// <inheritdoc/>
        public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
        {
            var clientOptions = options.Value;

            var key = message.GetSha1();
            var result = await cache.AddOrGet(key, async _ =>
            {
                try
                {
                    var response = await next(message, token);
                    return CachingResult.OfValue((dynamic)response!);
                }
                catch (Exception ex)
                {
                    if (ex is not MessageDeferredException && !clientOptions.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                        return CachingResult.OfException(ex);
                    throw;
                }
            }, token);

            return (TResponse)result.GetValue();
        }
    }
}
