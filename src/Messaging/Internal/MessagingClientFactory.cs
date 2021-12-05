using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Default messaging client factory implementation.
    /// </summary>
    internal class MessagingClientFactory : IMessagingClientFactory
    {
        private readonly IServiceProvider provider;

        /// <summary/>
        public MessagingClientFactory(IServiceProvider provider) =>
            this.provider = provider;

        public IMessagingClient Create() => Create(Microsoft.Extensions.Options.Options.DefaultName);

        public IMessagingClient Create(string name) => new MessagingClient(name, provider);
    }
}
