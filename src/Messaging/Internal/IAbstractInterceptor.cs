using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    internal interface IAbstractInterceptor
    {
        Task<object> Intercept(object command, Func<object, Task<object>> next);
    }
}