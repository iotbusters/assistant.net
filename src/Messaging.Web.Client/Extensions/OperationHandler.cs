using Assistant.Net.Diagnostics.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     Operation tracking for remote message handling.
    /// </summary>
    public class OperationHandler : DelegatingHandler
    {
        private readonly IDiagnosticFactory diagnosticsFactory;

        /// <summary/>
        public OperationHandler(IDiagnosticFactory diagnosticsFactory) =>
            this.diagnosticsFactory = diagnosticsFactory;

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var messageName = request.GetMessageName().ToLower();
            var operation = diagnosticsFactory.Start($"{messageName}-remote-client-handling");

            try
            {
                return base.SendAsync(request, cancellationToken);
            }
            catch
            {
                operation.Fail();
                throw;
            }
            finally
            {
                operation.Complete();
            }
        }
    }
}