using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    public class TimeoutInterceptor : ICommandInterceptor
    {
        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            var timeout = TimeSpan.FromSeconds(30);// configurable

            try
            {
                return await Task.Run(() => next(command), new CancellationTokenSource(timeout).Token);
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
                throw;
            }
        }
    }
}