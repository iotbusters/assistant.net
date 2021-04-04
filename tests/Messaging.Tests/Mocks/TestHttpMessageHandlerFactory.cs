using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public class TestHttpMessageHandlerFactory : IHttpMessageHandlerFactory
    {
        private readonly HttpMessageHandler handler;

        public TestHttpMessageHandlerFactory(HttpMessageHandler handler) =>
            this.handler = handler;

        public HttpMessageHandler CreateHandler(string name) => handler;
    }
}