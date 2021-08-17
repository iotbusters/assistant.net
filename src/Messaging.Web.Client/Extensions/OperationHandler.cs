using Assistant.Net.Diagnostics.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     Operation tracking for remote command handling.
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
            var commandName = request.GetCommandName().ToLower();
            var operation = diagnosticsFactory.Start($"{commandName}-remote-client-handling");

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