using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Configuration with WEB server set of interceptors:
///     <see cref="DiagnosticsInterceptor"/>, <see cref="ErrorHandlingInterceptor"/>,
///     <see cref="CachingInterceptor"/>, <see cref="DeferredCachingInterceptor"/>,
///     <see cref="TimeoutInterceptor"/>
/// </summary>
public sealed class WebServerInterceptorConfiguration : IMessageConfiguration
{
    /// <inheritdoc/>
    public void Configure(MessagingClientBuilder builder)
    {
        builder.Services.AddStorage(builder.Name, b => b.AddLocal<IAbstractMessage, CachingResult>());
        builder
            .ClearInterceptors()
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<CachingInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<DeferredCachingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>()
            .ClearExposedExceptions()
            .ClearTransientExceptions();
    }
}
