using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics.Tests.Mocks
{
    public class TestDiagnosticContext : IDiagnosticContext
    {
        public string? CorrelationId => default;
        public string? User => default;
    }
}
