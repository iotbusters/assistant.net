using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures
{
    public class MessageClientFixture : IDisposable
    {
        private readonly ServiceProvider provider;
        private readonly IHost host;

        public MessageClientFixture(ServiceProvider provider, IHost host)
        {
            this.provider = provider;
            this.host = host;
        }

        public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

        public virtual void Dispose()
        {
            provider.Dispose();
            host.Dispose();
        }
    }
}
