using System;
using System.Threading.Tasks;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    public class OperationInterceptor : ICommandInterceptor<ICommand<object>, object>
    {
        private readonly IOperationFactory operationFactory;

        public OperationInterceptor(IOperationFactory operationFactory) =>
            this.operationFactory = operationFactory;

        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            var operation = operationFactory.Start($"{command.GetType().Name.ToLower()}-handling");
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