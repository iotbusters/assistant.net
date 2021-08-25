using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using System;
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
        public async Task<object> Intercept(IMessage<object> message, Func<IMessage<object>, Task<object>> next)
        {
            var messageName = message.GetType().Name.ToLower();
            var operation = diagnosticsFactory.Start($"{messageName}-local-handling");

            try
            {
                return await next(message);
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