using System;
using System.Threading.Tasks;
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
                OverrideException(ex);
                throw;
            }
        }

        private static void OverrideException(Exception ex)
        {
            switch (ex)
            {
                case TaskCanceledException:
                case OperationCanceledException:
                case TimeoutException:
                case CommandException:
                    return;

                case AggregateException e:
                    OverrideException(e.InnerException!);
                    return;

                default:
                    throw new CommandFailedException(ex);
            };
        }
    }
}