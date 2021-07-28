using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    public sealed class CachingResult
    {
        public CachingResult() { } // serialization
        public CachingResult(object value) => Value = value;
        public CachingResult(Exception exception) => Exception = exception;

        public object? Value { get; set; } // serialization
        public Exception? Exception { get; set; } // serialization

        public object GetResult()
        {
            Exception?.Throw();
            return Value!;
        }

        public Task<object> GetTask()
        {
            if (Exception != null)
                return Task.FromException<object>(Exception!);
            return Task.FromResult(Value!);
        }
    }
}