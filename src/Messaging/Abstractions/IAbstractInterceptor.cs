using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     De-typed interceptor abstraction that helps intercepting unknown commands 
    ///     during runtime without reflection related performance drop.
    /// </summary>
    public interface IAbstractInterceptor
    {
        Task<object> Intercept(object command, Func<object, Task<object>> next);
    }
}