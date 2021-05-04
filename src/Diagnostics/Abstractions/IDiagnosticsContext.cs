using System;

namespace Assistant.Net.Diagnostics.Abstractions
{
    public interface IDiagnosticsContext
    {
        Guid CorrelationId { get; }
    }
}