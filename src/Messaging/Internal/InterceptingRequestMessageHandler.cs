using Assistant.Net.Messaging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal struct InterceptingRequestMessageHandler
{
    private readonly IAbstractHandler handler;
    private readonly IEnumerator<IAbstractRequestInterceptor> interceptors;

    public InterceptingRequestMessageHandler(IAbstractHandler handler, IEnumerable<IAbstractRequestInterceptor> interceptors)
    {
        this.handler = handler;
        this.interceptors = interceptors.GetEnumerator();
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (interceptors.MoveNext())
            return await interceptors.Current.Intercept(Request, message, token);

        return await handler.Request(message, token);
    }
}
