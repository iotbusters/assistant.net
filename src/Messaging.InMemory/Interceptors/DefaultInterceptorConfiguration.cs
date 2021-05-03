using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Interceptors
{
    public class DefaultInterceptorConfiguration : ICommandConfiguration
    {
        public void Configure(CommandOptions options) => options.Interceptors
            .Add<OperationInterceptor>()
            .Add<ErrorHandlingInterceptor>()
            .Add<TimeoutInterceptor>()
            .Add<CachingInterceptor>()
            .Add<RetryingInterceptor>();
    }
}