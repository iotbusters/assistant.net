using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Sqlite.Tests.Mocks;

public class TestNestedMessageHandler : IMessageHandler<TestNestedRequest, TestResponse>, IMessageHandler<TestNestedEvent>
{
    private readonly IMessagingClient client;

    public TestNestedMessageHandler(IMessagingClient client) =>
        this.client = client;

    public async Task<TestResponse> Handle(TestNestedRequest message, CancellationToken token) =>
        await client.Request(new TestRequest(), token);

    public async Task Handle(TestNestedEvent message, CancellationToken token = default) =>
        await client.Publish(new TestEvent(), token);
}
