using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using System;

namespace Assistant.Net.Messaging.Web.Tests.Mocks;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpClientRedirect<TImplementation>(this IServiceCollection services, IHost host) => services
        .AddHttpClientRedirect<TImplementation>(_ => host);

    public static IServiceCollection AddHttpClientRedirect<TImplementation>(this IServiceCollection services, Func<IServiceProvider, IHost> hostFactory) => services
        .AddSingleton(hostFactory)
        .Configure<HttpClientFactoryOptions, IHost>(typeof(TImplementation).Name, (options, host) => options
            .HttpMessageHandlerBuilderActions.Add(builder => builder
                .PrimaryHandler = host.GetTestServer().CreateHandler()));
}
