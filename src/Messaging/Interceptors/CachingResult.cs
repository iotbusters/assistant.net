using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Execution result for caching mechanism.
    /// </summary>
    public sealed class CachingResult
    {
        /// <summary>
        ///     Ctor for successful result.
        /// </summary>
        public CachingResult(object value) => Value = value;

        /// <summary>
        ///     Ctor for failed result.
        /// </summary>
        public CachingResult(Exception exception) => Exception = exception;

        /// <summary>
        ///     Successful result value.
        /// </summary>
        private object? Value { get; }

        /// <summary>
        ///     Failed result exception.
        /// </summary>
        private Exception? Exception { get; }

        /// <summary>
        ///     Restores result value or throws occurred exception.
        /// </summary>
        public object GetValue()
        {
            Exception?.Throw();
            return Value!;
        }

        /// <summary>
        ///     Restores result value or throws occurred exception.
        /// </summary>
        public Task<object> GetValueAsTask()
        {
            if (Exception != null)
                return Task.FromException<object>(Exception!);
            return Task.FromResult(Value!);
        }
    }
}