using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Configuration with server set of interceptors:
    ///     <see cref="DiagnosticsInterceptor" />, <see cref="ErrorHandlingInterceptor" />,
    ///     <see cref="CachingInterceptor" />, <see cref="DeferredCachingInterceptor" />,
    ///     <see cref="TimeoutInterceptor" />
    /// </summary>
    public class ServerInterceptorConfiguration : ICommandConfiguration
    {
        // todo: consider removing if disabled in configuration (https://github.com/iotbusters/assistant.net/issues/1)
        public void Configure(CommandClientBuilder builder) => builder
            .ClearAll()
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<CachingInterceptor>()
            .AddInterceptor<DeferredCachingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>();
    }
}