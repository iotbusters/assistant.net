using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Global error handling interceptor.
    /// </summary>
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
        ///     Converts any occurred exception to <see cref="CommandException" /> due to convention.
        /// </summary>
        private static Exception ToCommandException(Exception ex)
        {
            // todo: resolve duplication in RetryingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
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