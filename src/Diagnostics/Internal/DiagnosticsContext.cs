using System;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics.Internal
{
    internal sealed class DiagnosticsContext : IDiagnosticsContext
    {
        public Guid CorrelationId { get; set; }
    }
}