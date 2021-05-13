using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     [NOT IMPLEMENTED]
    ///     Default web authorizing implementation for remote command handling.
    /// </summary>
    internal class AuthorizationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // todo: add authentication and authorization (https://github.com/iotbusters/assistant.net/issues/5)
            return base.SendAsync(request, cancellationToken);
        }
    }
}