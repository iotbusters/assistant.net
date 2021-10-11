using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Configuration with default set of interceptors:
    ///     <see cref="DiagnosticsInterceptor" />, <see cref="ErrorHandlingInterceptor" />,
    ///     <see cref="CachingInterceptor" />, <see cref="RetryingInterceptor" />,
    ///     <see cref="TimeoutInterceptor" />
    /// </summary>
    public class DefaultInterceptorConfiguration : IMessageConfiguration
    {
        /// <inheritdoc/>
        public void Configure(MessagingClientBuilder builder)
        {
            builder.Services.AddStorage(b => b.AddLocal<string, CachingResult>());
            builder
                .ClearInterceptors()
                .AddInterceptor<DiagnosticsInterceptor>()
                .AddInterceptor<CachingInterceptor>()
                .AddInterceptor<ErrorHandlingInterceptor>()
                .AddInterceptor<RetryingInterceptor>()
                .AddInterceptor<TimeoutInterceptor>()
                .ClearExposedExceptions()
                .ExposeException<TimeoutException>()
                .ExposeException<OperationCanceledException>()
                .ClearTransientExceptions();
        }
    }
}
