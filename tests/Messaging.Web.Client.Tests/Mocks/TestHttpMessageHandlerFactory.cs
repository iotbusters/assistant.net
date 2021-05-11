using System.Net.Http;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks
{
    public class TestHttpMessageHandlerFactory : IHttpMessageHandlerFactory
    {
        private readonly HttpMessageHandler handler;

        public TestHttpMessageHandlerFactory(HttpMessageHandler handler) =>
            this.handler = handler;

        public HttpMessageHandler CreateHandler(string name) => handler;
    }
}