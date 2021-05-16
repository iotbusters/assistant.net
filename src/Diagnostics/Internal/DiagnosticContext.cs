using System;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics.Internal
{
    /// <summary>
    ///     Default diagnostic context implementation.
    /// </summary>
    internal sealed class DiagnosticContext : IDiagnosticsContext
    {
        public Guid CorrelationId { get; set; }
    }
}