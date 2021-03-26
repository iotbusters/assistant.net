using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    public class ScopedCommandInterceptor : ICommandInterceptor
    {
        public Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            throw new NotImplementedException();
        }
    }
}