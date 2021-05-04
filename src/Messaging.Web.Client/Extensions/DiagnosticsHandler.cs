using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Messaging.Extensions
{
    internal class DiagnosticsHandler : DelegatingHandler
    {
        private readonly IDiagnosticsFactory diagnosticsFactory;

        public DiagnosticsHandler(IDiagnosticsFactory diagnosticsFactory) =>
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