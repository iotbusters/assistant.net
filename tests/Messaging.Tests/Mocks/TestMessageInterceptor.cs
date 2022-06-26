using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestMessageInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    public Task<TResponse> Intercept(MessageInterceptor<TMessage, TResponse> next, TMessage message, CancellationToken token)
    {
        CallCount++;
        return next(message, token);
    }

    public int CallCount { get; private set; }
}
