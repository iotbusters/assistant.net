using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Tests.Fixtures
{
    public class MessageClientFixture : IDisposable
    {
        private readonly ServiceProvider provider;

        public MessageClientFixture(ServiceProvider provider)
        {
            this.provider = provider;
        }

        public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

        public virtual void Dispose() => provider.Dispose();
    }
}