using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestAbstractInterceptor : IAbstractInterceptor
{
    public Task<object> Intercept(MessageInterceptor next, IAbstractMessage message, CancellationToken token)
    {
        CallCount++;
        return next(message, token);
    }

    public int CallCount { get; private set; }
}
