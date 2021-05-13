using System;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics.Internal
{
    /// <summary>
    ///     Default diagnostic context implementation.
    /// </summary>
    internal sealed class DiagnosticsContext : IDiagnosticsContext
    {
        public Guid CorrelationId { get; set; }
    }
}