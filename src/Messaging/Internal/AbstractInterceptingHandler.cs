using Assistant.Net.Messaging.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    internal class AbstractInterceptingHandler : IAbstractHandler
    {
        private readonly IAbstractHandler handler;
        private readonly IEnumerator<IAbstractInterceptor> interceptors;

        public AbstractInterceptingHandler(IAbstractHandler handler, IEnumerable<IAbstractInterceptor> interceptors)
        {
            this.handler = handler;
            this.interceptors = interceptors.GetEnumerator();
        }

        public async Task<object> Handle(object message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (interceptors.MoveNext())
                return await interceptors.Current.Intercept(Handle, message, token);

            return (await handler.Handle(message, token))!;
        }
    }
}
