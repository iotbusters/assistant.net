using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     Spreads current correlation context on remote command handling.
    /// </summary>
    internal class CorrelationHandler : DelegatingHandler
    {
        private readonly IDiagnosticContext context;

        public CorrelationHandler(IDiagnosticContext context) =>
            this.context = context;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetCorrelationId(context.CorrelationId);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}