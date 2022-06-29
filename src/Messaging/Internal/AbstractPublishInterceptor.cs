using Assistant.Net.Messaging.Abstractions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

[StackTraceHidden]
internal class AbstractPublishInterceptor<TMessage> : IAbstractPublishInterceptor where TMessage : IAbstractMessage
{
    private readonly IMessagePublishInterceptor<TMessage> interceptor;

    public AbstractPublishInterceptor(IMessagePublishInterceptor<TMessage> interceptor) =>
        this.interceptor = interceptor;

    public ValueTask Intercept(PublishMessageHandler next, IAbstractMessage message, CancellationToken token) =>
        interceptor.Intercept((m, t) => next(m, t), (TMessage)message, token);
}
