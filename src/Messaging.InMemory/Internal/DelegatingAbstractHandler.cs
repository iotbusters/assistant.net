using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    internal class DelegatingAbstractHandler : IAbstractHandler
    {
        private readonly Func<object, Task<object>> handle;

        public DelegatingAbstractHandler(Func<object, Task<object>> handle) =>
            this.handle = handle;

        public Task<object> Handle(object command) => handle(command);
    }
}