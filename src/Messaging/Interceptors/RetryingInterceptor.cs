using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.Logging;

namespace Assistant.Net.Messaging.Interceptors
{
    public class RetryingInterceptor : ICommandInterceptor
    {
        private readonly ILogger<RetryingInterceptor> logger;
        private readonly ISystemLifetime lifetime;

        public RetryingInterceptor(
            ILogger<RetryingInterceptor> logger,
            ISystemLifetime lifetime)
        {
            this.lifetime = lifetime;
            this.logger = logger;
        }

        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            // configurable
            var maxRetryLimit = 10;
            var delayingStrategy = new Func<int, TimeSpan>(x => TimeSpan.FromSeconds(Math.Pow(x, 2)));
            var criticalExceptionTypes = new[]
            {
                typeof(TaskCanceledException),
                typeof(OperationCanceledException),
                typeof(TimeoutException),
                typeof(CommandException)
            };

            var retry = 0;
            do
            {
                lifetime.Stopping.ThrowIfCancellationRequested();
                try
                {
                    return await next(command);
                }
                catch (AggregateException ex)
                {
                    if (criticalExceptionTypes.Any(x => x.IsAssignableFrom(ex.InnerException!.GetType())))
                        throw;

                    if (retry == 0)
                        logger.LogWarning(ex.InnerException, "Transient error occurred.");
                    else
                        logger.LogWarning(ex.InnerException, $"Transient error occurred. Retry #{retry}.");

                    if (retry == maxRetryLimit)
                        throw new CommandRetryLimitExceededException(ex);

                    retry++;
                    await Task.Delay(delayingStrategy(retry));
                }
            }
            while (true);
        }
    }
}