using Assistant.Net.Messaging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal struct InterceptingPublishMessageHandler
{
    private readonly IAbstractHandler handler;
    private readonly IEnumerator<IAbstractPublishInterceptor> interceptors;

    public InterceptingPublishMessageHandler(IAbstractHandler handler, IEnumerable<IAbstractPublishInterceptor> interceptors)
    {
        this.handler = handler;
        this.interceptors = interceptors.GetEnumerator();
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (interceptors.MoveNext())
        {
            await interceptors.Current.Intercept(Publish, message, token);
            return;
        }

        await handler.Publish(message, token);
    }
}
