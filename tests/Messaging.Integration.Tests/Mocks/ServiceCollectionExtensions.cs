using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace Assistant.Net.Messaging.Integration.Tests.Mocks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpClientRedirect<TImplementation>(this IServiceCollection services, IHost host) => services
            .AddHttpClientRedirect<TImplementation>(p => host);

        public static IServiceCollection AddHttpClientRedirect<TImplementation>(this IServiceCollection services, Func<IServiceProvider, IHost> hostFactory) => services
            .AddSingleton(hostFactory)
            .Configure<HttpClientFactoryOptions, IHost>(typeof(TImplementation).Name, (options, host) => options
                .HttpMessageHandlerBuilderActions.Add(builder => builder
                    .PrimaryHandler = host.GetTestServer().CreateHandler()));
    }
}