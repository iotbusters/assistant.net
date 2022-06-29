using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks;

internal class TestAbstractHandler : IAbstractHandler
{
    private readonly dynamic handlerInstance;

    public TestAbstractHandler(object handlerInstance) =>
        this.handlerInstance = handlerInstance;

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token) =>
        await handlerInstance.Handle(message, token);

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token) =>
        await handlerInstance.Handle(message, token);
}
