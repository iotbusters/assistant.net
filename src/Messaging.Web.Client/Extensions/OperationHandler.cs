using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     Operation tracking for remote command handling.
    /// </summary>
    internal class OperationHandler : DelegatingHandler
    {
        private readonly IDiagnosticsFactory diagnosticsFactory;

        public OperationHandler(IDiagnosticsFactory diagnosticsFactory) =>
            this.diagnosticsFactory = diagnosticsFactory;

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