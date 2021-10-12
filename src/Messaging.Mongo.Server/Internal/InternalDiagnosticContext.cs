using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Diagnostic context implementation for internal purpose.
    /// </summary>
    internal class InternalDiagnosticContext : IDiagnosticContext
    {
        public string CorrelationId { get; set; } = default!;
    }
}
