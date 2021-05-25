using System;
using System.Runtime.ExceptionServices;

namespace Assistant.Net.Messaging.Caching
{
    public class Result
    {
        protected readonly object? value;
        protected readonly Exception? exception;

        public Result(object value) => this.value = value;

        public Result(Exception exception) => this.exception = exception;

        public object Get()
        {
            if (exception != null)
                ExceptionDispatchInfo.Capture(exception).Throw();
            return value!;
        }
    }
}