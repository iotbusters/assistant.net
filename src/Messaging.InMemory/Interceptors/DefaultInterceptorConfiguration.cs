using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     todo: consider removing if disabled in configuration (https://github.com/iotbusters/assistant.net/issues/1)
    /// </summary>
    public class DefaultInterceptorConfiguration : ICommandConfiguration
    {
        public void Configure(CommandOptions options) => options.Interceptors
            .Add<DiagnosticsInterceptor>()
            .Add<ErrorHandlingInterceptor>()
            .Add<CachingInterceptor>()
            .Add<RetryingInterceptor>()
            .Add<TimeoutInterceptor>();
    }
}