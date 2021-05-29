using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Integration.Tests.Fixtures
{
    public class CommandClientFixture : IDisposable
    {
        private readonly ServiceProvider provider;

        public CommandClientFixture(ServiceProvider provider)
        {
            this.provider = provider;
        }

        public ICommandClient Client => provider.GetRequiredService<ICommandClient>();

        public virtual void Dispose() => provider.Dispose();
    }
}