using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestAbstractInterceptor : SharedAbstractInterceptor
{
    protected override ValueTask<object> InterceptInternal(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        CallCount++;
        return next(message, token);
    }

    public int CallCount { get; private set; }
}
