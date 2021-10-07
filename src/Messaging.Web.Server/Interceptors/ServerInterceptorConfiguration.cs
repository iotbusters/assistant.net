using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Configuration with server set of interceptors:
    ///     <see cref="DiagnosticsInterceptor" />, <see cref="ErrorHandlingInterceptor" />,
    ///     <see cref="CachingInterceptor" />, <see cref="DeferredCachingInterceptor" />,
    ///     <see cref="TimeoutInterceptor" />
    /// </summary>
    public class ServerInterceptorConfiguration : IMessageConfiguration
    {
        /// <inheritdoc/>
        public void Configure(MessagingClientBuilder builder)
        {
            builder.Services.AddStorage(b => b.AddLocal<string, CachingResult>());
            builder
                .ClearInterceptors()
                .AddInterceptor<DiagnosticsInterceptor>()
                .AddInterceptor<ErrorHandlingInterceptor>()
                .AddInterceptor<CachingInterceptor>()
                .AddInterceptor<DeferredCachingInterceptor>()
                .AddInterceptor<TimeoutInterceptor>();
        }
    }
}
