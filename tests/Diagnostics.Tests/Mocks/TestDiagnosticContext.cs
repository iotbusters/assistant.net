using Assistant.Net.Diagnostics.Abstractions;

namespace Diagnostics.Tests.Mocks
{
    public class TestDiagnosticContext : IDiagnosticContext
    {
        public string CorrelationId { get; } = default!;
    }
}
