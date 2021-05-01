using System;

namespace Assistant.Net.Diagnostics.Abstractions
{
    public interface IOperationContext
    {
        Guid CorrelationId { get; }
    }
}