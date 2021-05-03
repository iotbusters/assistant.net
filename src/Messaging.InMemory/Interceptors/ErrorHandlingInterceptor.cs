using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Interceptors
{
    public class ErrorHandlingInterceptor : ICommandInterceptor<ICommand<object>, object>
    {
        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            try
            {
                return await next(command);
            }
            catch (Exception ex)
            {
                var exception = ToCommandException(ex);
                ExceptionDispatchInfo.Capture(exception).Throw();
                throw;
            }
        }

        /// <summary>
        ///     todo: resolve duplication in RetryingInterceptor.
        /// </summary>
        private static Exception ToCommandException(Exception ex)
        {
            // configurable
            var criticalExceptionTypes = new[]
            {
                typeof(TaskCanceledException),
                typeof(OperationCanceledException),
                typeof(TimeoutException),
                typeof(CommandException)
            };

            if (ex is AggregateException e)
                return ToCommandException(e.InnerException!);

            if (criticalExceptionTypes.Any(x => x.IsAssignableFrom(ex.GetType())))
                return ex;

            return new CommandFailedException(ex);
        }
    }
}