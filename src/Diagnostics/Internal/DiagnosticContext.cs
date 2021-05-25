using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics.Internal
{
    /// <summary>
    ///     Default diagnostic context implementation.
    /// </summary>
    internal sealed class DiagnosticContext : IDiagnosticContext
    {
        public string CorrelationId { get; set; } = null!;
    }
}