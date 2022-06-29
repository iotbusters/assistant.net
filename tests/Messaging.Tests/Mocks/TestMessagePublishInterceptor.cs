using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestMessagePublishInterceptor<TMessage> : IMessagePublishInterceptor<TMessage>
    where TMessage : IAbstractMessage
{
    public ValueTask Intercept(PublishMessageHandler<TMessage> next, TMessage message, CancellationToken token)
    {
        CallCount++;
        return next(message, token);
    }

    public int CallCount { get; private set; }
}
