using System;
using Assistant.Net.Diagnostics.Abstractions;

namespace Assistant.Net.Diagnostics.Internal
{
    internal sealed class OperationContext : IOperationContext
    {
        public Guid CorrelationId { get; set; }
    }
}