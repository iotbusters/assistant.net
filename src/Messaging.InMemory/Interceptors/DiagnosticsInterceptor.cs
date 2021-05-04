using System;
using System.Threading.Tasks;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    public class DiagnosticsInterceptor : ICommandInterceptor<ICommand<object>, object>
    {
        private readonly IDiagnosticsFactory diagnosticsFactory;

        public DiagnosticsInterceptor(IDiagnosticsFactory diagnosticsFactory) =>
            this.diagnosticsFactory = diagnosticsFactory;

        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            var commandName = command.GetType().Name.ToLower();
            var operation = diagnosticsFactory.Start($"{commandName}-local-handling");

            try
            {
                return await next(command);
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