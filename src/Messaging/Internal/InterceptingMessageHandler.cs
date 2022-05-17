using Assistant.Net.Messaging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal class InterceptingMessageHandler
{
    private readonly IAbstractHandler handler;
    private readonly IEnumerator<IAbstractInterceptor> interceptors;

    public InterceptingMessageHandler(IAbstractHandler handler, IEnumerable<IAbstractInterceptor> interceptors)
    {
        this.handler = handler;
        this.interceptors = interceptors.GetEnumerator();
    }

    public async Task<object> Request(object message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (interceptors.MoveNext())
            return await interceptors.Current.Intercept(Request, message, token);

        return (await handler.Request(message, token))!;
    }

    public async Task Publish(object message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (interceptors.MoveNext())
        {
            await interceptors.Current.Intercept(Request, message, token);
            return;
        }

        await handler.Publish(message, token);
    }
}