using System;
using System.Threading.Tasks;
using Assistant.Net.Diagnostics.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Operation tracking middleware.
    /// </summary>
    internal class DiagnosticMiddleware
    {
        private readonly RequestDelegate next;

        public DiagnosticMiddleware(RequestDelegate next) =>
            this.next = next;

        public async Task Invoke(HttpContext context, IDiagnosticsFactory operationFactory)
        {
            var commandName = context.GetCommandName().ToLower();

            var operation = operationFactory.Start($"{commandName}-remote-server-handling");

            try
            {
                await next(context);
            }
            catch (Exception)
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
