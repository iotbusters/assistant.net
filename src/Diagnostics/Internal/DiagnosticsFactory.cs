using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;

namespace Assistant.Net.Diagnostics.Internal
{
    internal sealed class DiagnosticsFactory : IDiagnosticsFactory, IDisposable
    {
        public DiagnosticsFactory(OperationEventSource eventSource, ISystemClock clock, IDiagnosticsContext context)
        {
            EventSource = eventSource;
            Clock = clock;
            Context = context;
        }

        internal OperationEventSource EventSource { get; }
        internal ISystemClock Clock { get; }
        internal IDiagnosticsContext Context { get; }
        internal IDictionary<string, IDisposable> Operations { get; } = new ConcurrentDictionary<string, IDisposable>();

        IOperation IDiagnosticsFactory.Start(string name) =>
            new Operation(name, this);

        void IDisposable.Dispose()
        {
            foreach (var key in Operations.Keys)
                if (Operations.TryGetValue(key, out var disposable))
                    disposable.Dispose();
        }
    }
}