using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Assistant.Net.Diagnostics.Internal;

/// <summary>
///     Default diagnostic factory implementation.
/// </summary>
internal sealed class DiagnosticFactory : IDiagnosticFactory, IDisposable
{
    public DiagnosticFactory(OperationEventSource eventSource, ISystemClock clock, IDiagnosticContext context)
    {
        EventSource = eventSource;
        Clock = clock;
        Context = context;
    }

    internal OperationEventSource EventSource { get; }
    internal ISystemClock Clock { get; }
    internal IDiagnosticContext Context { get; }
    internal IDictionary<string, IDisposable> Operations { get; } = new ConcurrentDictionary<string, IDisposable>();

    IOperation IDiagnosticFactory.Start(string name) => new Operation(name, this);

    void IDisposable.Dispose()
    {
        foreach (var key in Operations.Keys)
            if (Operations.Remove(key, out var disposable))
                disposable.Dispose();
    }
}
