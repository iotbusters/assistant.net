using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.Logging;

namespace Assistant.Net.Messaging.Interceptors
{
    public class RetryingCommandInterceptor : ICommandInterceptor
    {
        private readonly ILogger<RetryingCommandInterceptor> logger;
        private readonly ISystemLifetime lifetime;

        public RetryingCommandInterceptor(
            ILogger<RetryingCommandInterceptor> logger,
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

            var retry = 0;
            do
            {
                lifetime.Stopping.ThrowIfCancellationRequested();
                try
                {
                    return await next(command);
                }
                catch(AggregateException ex) when (ex.InnerException is TaskCanceledException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }
                catch(AggregateException ex) when (ex.InnerException is TimeoutException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }
                catch (AggregateException ex)
                {
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