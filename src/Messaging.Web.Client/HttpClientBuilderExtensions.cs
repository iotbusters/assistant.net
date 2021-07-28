using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System.Linq;
using System.Net.Http;

namespace Assistant.Net.Messaging
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        ///     Adds an additional <typeparamref name="THandler" /> to http message handling pipeline.
        /// </summary>
        public static IHttpClientBuilder AddHttpMessageHandler<THandler>(this IHttpClientBuilder builder)
            where THandler : DelegatingHandler
        {
            builder.Services.AddTransient<THandler>();
            return Microsoft.Extensions.DependencyInjection.HttpClientBuilderExtensions.AddHttpMessageHandler<THandler>(builder);
        }

        /// <summary>
        ///     Removes <typeparamref name="THandler" /> from http message handling pipeline.
        /// </summary>
        public static IHttpClientBuilder RemoveHttpMessageHandler<THandler>(this IHttpClientBuilder builder)
            where THandler : DelegatingHandler
        {
            builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b =>
                {
                    var handlers = b.AdditionalHandlers.Where(x => x is THandler).ToArray();
                    foreach (var handler in handlers)
                        b.AdditionalHandlers.Remove(handler);
                });
            });
            return builder;
        }

        /// <summary>
        ///     Removes all <see cref="DelegatingHandler" />s from http message handling pipeline.
        /// </summary>
        public static IHttpClientBuilder ClearAllHttpMessageHandlers(this IHttpClientBuilder builder)
        {
            builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options => options
                .HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Clear()));
            return builder;
        }
    }
}