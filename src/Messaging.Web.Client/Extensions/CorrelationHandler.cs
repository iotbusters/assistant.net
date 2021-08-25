using Assistant.Net.Diagnostics.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     Spreads current correlation context on remote message handling.
    /// </summary>
    public class CorrelationHandler : DelegatingHandler
    {
        private readonly IDiagnosticContext context;

        /// <summary/>
        public CorrelationHandler(IDiagnosticContext context) =>
            this.context = context;

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetCorrelationId(context.CorrelationId);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}