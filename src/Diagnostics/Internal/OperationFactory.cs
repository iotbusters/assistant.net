using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;

namespace Assistant.Net.Diagnostics.Internal
{
    internal sealed class OperationFactory : IOperationFactory, IDisposable
    {
        public OperationFactory(OperationEventSource eventSource, ISystemClock clock, IOperationContext context)
        {
            EventSource = eventSource;
            Clock = clock;
            Context = context;
        }

        internal OperationEventSource EventSource { get; }
        internal ISystemClock Clock { get; }
        internal IOperationContext Context { get; }
        internal IDictionary<string, IDisposable> Operations { get; } = new ConcurrentDictionary<string, IDisposable>();

        IOperation IOperationFactory.Start(string name) =>
            new Operation(name, this);

        void IDisposable.Dispose()
        {
            foreach (var key in Operations.Keys)
                if (Operations.TryGetValue(key, out var disposable))
                    disposable.Dispose();
        }
    }
}