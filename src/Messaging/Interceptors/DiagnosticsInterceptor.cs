using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Operation tracking interceptor.
    /// </summary>
    public class DiagnosticsInterceptor : IMessageInterceptor<IMessage<object>, object>
    {
        private readonly IDiagnosticFactory diagnosticsFactory;

        /// <summary/>
        public DiagnosticsInterceptor(IDiagnosticFactory diagnosticsFactory) =>
            this.diagnosticsFactory = diagnosticsFactory;

        /// <inheritdoc/>
        public async Task<object> Intercept(
            Func<IMessage<object>, CancellationToken, Task<object>> next,
            IMessage<object> message,
            CancellationToken token)
        {
            var messageName = message.GetType().Name.ToLower();
            var operation = diagnosticsFactory.Start($"{messageName}-local-handling");

            try
            {
                return await next(message, token);
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