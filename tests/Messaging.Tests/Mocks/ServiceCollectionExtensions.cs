using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpClientRedirect<TImplementation>(this IServiceCollection services, IHost host)
        {
            var handler = host.GetTestServer().CreateHandler();
            return services
                .Configure<HttpClientFactoryOptions>(typeof(TImplementation).Name, options => options
                    .HttpMessageHandlerBuilderActions.Add(builder => builder.PrimaryHandler = handler));
        }
    }
}