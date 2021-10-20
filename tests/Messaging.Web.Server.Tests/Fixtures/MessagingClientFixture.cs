using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging.Web.Server.Tests.Fixtures
{
    public class MessagingClientFixture : IDisposable
    {
        private readonly ServiceProvider provider;
        private readonly IHost host;

        public MessagingClientFixture(ServiceProvider provider, IHost host)
        {
            this.provider = provider;
            this.host = host;
        }

        public HttpClient Client => provider.GetRequiredService<HttpClient>();

        public JsonSerializerOptions JsonSerializerOptions => provider.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;

        public virtual void Dispose()
        {
            provider.Dispose();
            host.Dispose();
        }
    }
}
