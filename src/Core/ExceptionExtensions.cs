using System;
using System.Runtime.ExceptionServices;

namespace Assistant.Net
{
    public static class ExceptionExtensions
    {
        public static void Throw(this Exception exception) =>
            ExceptionDispatchInfo.Capture(exception).Throw();

        public static T Throw<T>(this Exception exception)
        {
            exception.Throw();
            throw exception; // unreachable;
        }
    }
}