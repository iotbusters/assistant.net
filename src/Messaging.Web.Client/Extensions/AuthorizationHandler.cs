using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions
{
    internal class AuthorizationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // todo: add authentication and authorization (https://github.com/iotbusters/assistant.net/issues/5)
            return base.SendAsync(request, cancellationToken);
        }
    }
}