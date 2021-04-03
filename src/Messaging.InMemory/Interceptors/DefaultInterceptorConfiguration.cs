using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;

namespace Assistant.Net.Messaging.Interceptors
{
    public class DefaultInterceptorConfiguration : ICommandConfiguration
    {
        public void Configure(CommandOptions options) => options.Interceptors
            .Add<ErrorHandlingInterceptor>()
            .Add<TimeoutInterceptor>()
            .Add<CachingInterceptor>()
            .Add<RetryingInterceptor>();
    }
}