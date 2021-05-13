using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

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
    }
}