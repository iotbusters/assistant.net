using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    internal class TestAbstractHandler : IAbstractHandler
    {
        private readonly dynamic handlerInstance;

        public TestAbstractHandler(object handlerInstance) =>
            this.handlerInstance = handlerInstance;

        public async Task<object> Request(object message, CancellationToken token) =>
            await handlerInstance.Handle(message, token);

        public async Task Publish(object message, CancellationToken token) =>
            await handlerInstance.Handle(message, token);
    }
}
