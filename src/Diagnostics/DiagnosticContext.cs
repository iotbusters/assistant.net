using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics
{
    /// <summary>
    ///     Default diagnostic context implementation.
    /// </summary>
    public sealed class DiagnosticContext : IDiagnosticContext
    {
        /// <inheritdoc/>
        public string? CorrelationId { get; set; }

        /// <inheritdoc/>
        public string? User { get; set; }
    }
}
