using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    internal class InternalDiagnosticContext : IDiagnosticContext
    {
        public string CorrelationId { get; set; } = default!;
    }
}
