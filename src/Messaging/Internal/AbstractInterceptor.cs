using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal class AbstractInterceptor<TInterceptor, TMessage, TResponse> : IAbstractInterceptor
    where TInterceptor : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly TInterceptor interceptor;

    public AbstractInterceptor(TInterceptor interceptor) =>
        this.interceptor = interceptor;

    public async Task<object> Intercept(Func<IAbstractMessage, CancellationToken, Task<object>> next, IAbstractMessage message, CancellationToken token) =>
        (await interceptor.Intercept(async (m, t) => (TResponse)await next(m, t), (TMessage)message, token))!;
}
