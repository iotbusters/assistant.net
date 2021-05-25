using System;
using System.Runtime.ExceptionServices;

namespace Assistant.Net.Messaging
{
    public class CachingResult
    {
        protected readonly object? value;
        protected readonly Exception? exception;

        public CachingResult(object value) => this.value = value;

        public CachingResult(Exception exception) => this.exception = exception;

        public object Get()
        {
            if (exception != null)
                ExceptionDispatchInfo.Capture(exception).Throw();
            return value!;
        }
    }
}