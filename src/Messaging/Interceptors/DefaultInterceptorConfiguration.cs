using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Configuration with default set of interceptors:
    ///     <see cref="DiagnosticsInterceptor" />, <see cref="ErrorHandlingInterceptor" />,
    ///     <see cref="CachingInterceptor" />, <see cref="RetryingInterceptor" />,
    ///     <see cref="TimeoutInterceptor" />
    /// </summary>
    public class DefaultInterceptorConfiguration : ICommandConfiguration
    {
        // todo: consider removing if disabled in configuration (https://github.com/iotbusters/assistant.net/issues/1)
        public void Configure(CommandClientBuilder builder) => builder
            .ClearAll()
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<CachingInterceptor>()
            .AddInterceptor<RetryingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>();
    }
}