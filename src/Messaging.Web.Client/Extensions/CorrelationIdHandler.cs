using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     Spreading current correlation context on remote command handling.
    /// </summary>
    internal class CorrelationIdHandler : DelegatingHandler
    {
        private readonly IDiagnosticsContext context;

        public CorrelationIdHandler(IDiagnosticsContext context) =>
            this.context = context;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetCorrelationId(context.CorrelationId);
            return base.SendAsync(request, cancellationToken);
        }
    }
}