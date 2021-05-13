using System;

namespace Assistant.Net.Diagnostics.Abstractions
{
    /// <summary>
    ///     Abstraction to diagnostic context which is responsible for sharing information
    ///     with all diagnostic services and other interested parties.
    /// </summary>
    public interface IDiagnosticsContext
    {
        /// <summary>
        ///     Current operation tracking identifier.
        /// </summary>
        Guid CorrelationId { get; }
    }
}