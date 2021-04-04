using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection.Hotfix
{
    internal static class HttpClientFactoryServiceCollectionExtensions
    {
        /// <summary>
        ///     The hotfix decouples <see cref="IHttpClientFactory"/> and <see cref="IHttpMessageHandlerFactory"/> implementations
        ///     to be managed independently. This may be important for some sophisticated behavior or test mocking.
        /// </summary>
        public static IHttpClientBuilder AddHttpClient<TImplementation>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
            where TImplementation : class
        {
            services.Replace(ServiceDescriptor.Singleton<IHttpClientFactory, DefaultHttpClientFactory>());
            return DependencyInjection.HttpClientFactoryServiceCollectionExtensions
                .AddHttpClient<TImplementation>(services, configureClient);
        }
    }
}