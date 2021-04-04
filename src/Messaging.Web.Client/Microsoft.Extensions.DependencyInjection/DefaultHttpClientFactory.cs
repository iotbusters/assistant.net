using System;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection.Hotfix
{
    /// <summary>
    ///     The hotfix decouples <see cref="DependencyInjection.DefaultHttpClientFactory"/> from <see cref="IHttpClientFactory"/> behavior
    ///     to be managed independently. This may be important for some sophisticated behavior or test mocking.
    /// </summary>
    internal class DefaultHttpClientFactory : IHttpClientFactory
    {
        private readonly IHttpMessageHandlerFactory handlerFactory;
        private readonly IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor;

        public DefaultHttpClientFactory(
            IHttpMessageHandlerFactory handlerFactory,
            IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor)
        {
            this.handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        public HttpClient CreateClient(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var handler = handlerFactory.CreateHandler(name);
            var client = new HttpClient(handler, disposeHandler: false);

            var options = optionsMonitor.Get(name);
            foreach (var action in options.HttpClientActions)
                action(client);

            return client;
        }
    }
}