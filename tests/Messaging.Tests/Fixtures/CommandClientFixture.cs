using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests.Fixtures
{
    public class CommandClientFixture : IDisposable
    {
        public ServiceProvider provider;

        public CommandClientFixture(ServiceProvider provider) =>
            this.provider = provider;

        public ICommandClient Client => provider.GetRequiredService<ICommandClient>();

        public virtual void Dispose()
        {
            provider.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}