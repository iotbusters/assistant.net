using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <inheritdoc cref="DiagnosticsInterceptor{TMessage,TResponse}"/>
    public class DiagnosticsInterceptor : DiagnosticsInterceptor<IMessage<object>, object>, IMessageInterceptor
    {
        /// <summary/>
        public DiagnosticsInterceptor(IDiagnosticFactory diagnosticFactory) : base(diagnosticFactory) { }
    }

    /// <summary>
    ///     Operation tracking interceptor.
    /// </summary>
    public class DiagnosticsInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly IDiagnosticFactory diagnosticFactory;

        /// <summary/>
        public DiagnosticsInterceptor(IDiagnosticFactory diagnosticFactory) =>
            this.diagnosticFactory = diagnosticFactory;

        /// <inheritdoc/>
        public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
        {
            var messageName = message.GetType().Name.ToLower();
            var operation = diagnosticFactory.Start($"{messageName}-handling-local");

            try
            {
                var response = await next(message, token);
                operation.Complete();
                return response;
            }
            catch (Exception)
            {
                operation.Fail();
                throw;
            }
        }
    }
}
