using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Global error handling interceptor.
    /// </summary>
    public class ErrorHandlingInterceptor : IMessageInterceptor<IMessage<object>, object>
    {
        /// <inheritdoc/>
        public async Task<object> Intercept(IMessage<object> message, Func<IMessage<object>, Task<object>> next)
        {
            try
            {
                return await next(message);
            }
            catch (Exception ex)
            {
                return ToMessageException(ex).Throw<object>();
            }
        }

        /// <summary>
        ///     Converts any occurred exception to <see cref="MessageException" /> due to convention.
        /// </summary>
        private static Exception ToMessageException(Exception ex)
        {
            // todo: resolve duplication in RetryingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
            // configurable
            var supportedExceptionTypes = new[]
            {
                typeof(OperationCanceledException),
                typeof(TimeoutException),
                typeof(MessageException)
            };

            if (ex is AggregateException e)
                return ToMessageException(e.InnerException!);

            if (supportedExceptionTypes.Any(x => x.IsInstanceOfType(ex)))
                return ex;

            return new MessageFailedException(ex);
        }
    }
}