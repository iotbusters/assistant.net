using Assistant.Net.Diagnostics.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

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

        public async Task Invoke(HttpContext context, IDiagnosticFactory operationFactory)
        {
            var messageName = context.GetMessageName().ToLower();

            var operation = operationFactory.Start($"{messageName}-remote-server-handling");

            try
            {
                await next(context);
            }
            catch (Exception)
            {
                operation.Fail();
                throw;
            }
            operation.Complete();
        }
    }
}
