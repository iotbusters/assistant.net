using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Function based implementation of de-typed command handler abstraction.
    /// </summary>
    internal class DelegatingAbstractHandler : IAbstractHandler
    {
        private readonly Func<object, Task<object>> handle;

        public DelegatingAbstractHandler(Func<object, Task<object>> handle) =>
            this.handle = handle;

        public Task<object> Handle(object command) => handle(command);
    }
}