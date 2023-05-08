using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Mongo.Tests.Mocks;

public class TestBackoffMessageHandler : IAbstractHandler
{
    public int CallCount { get; set; }

    public ValueTask<object> Request(IAbstractMessage message, CancellationToken token = default)
    {
        CallCount++;
        return ValueTask.FromResult<object>(Nothing.Instance);
    }

    public ValueTask Publish(IAbstractMessage message, CancellationToken token = default)
    {
        CallCount++;
        return ValueTask.CompletedTask;
    }
}
