using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestMessageRequestInterceptor<TMessage, TResponse> : IMessageRequestInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    public ValueTask<TResponse> Intercept(RequestMessageHandler<TMessage, TResponse> next, TMessage message, CancellationToken token)
    {
        CallCount++;
        return next(message, token);
    }

    public int CallCount { get; private set; }
}
