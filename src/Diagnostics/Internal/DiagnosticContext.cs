using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics.Internal
{
    /// <summary>
    ///     Default diagnostic context implementation.
    /// </summary>
    internal sealed class DiagnosticContext : IDiagnosticContext
    {
        /// <summary/>
        public DiagnosticContext(string correlationId) =>
            CorrelationId = correlationId;

        /// <summary>
        ///     Correlation ID in current operation scope.
        /// </summary>
        public string CorrelationId { get; }
    }
}
