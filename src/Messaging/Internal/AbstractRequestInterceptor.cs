using Assistant.Net.Messaging.Abstractions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

[StackTraceHidden]
internal class AbstractRequestInterceptor<TMessage, TResponse> : IAbstractRequestInterceptor
    where TMessage : IMessage<TResponse>
{
    private readonly IMessageRequestInterceptor<TMessage, TResponse> interceptor;

    public AbstractRequestInterceptor(IMessageRequestInterceptor<TMessage, TResponse> interceptor) =>
        this.interceptor = interceptor;

    public async ValueTask<object> Intercept(RequestMessageHandler next, IAbstractMessage message, CancellationToken token) =>
        (await interceptor.Intercept(async (m, t) => (TResponse)await next(m, t), (TMessage)message, token))!;
}
