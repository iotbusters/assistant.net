using System;

namespace Assistant.Net.Metrics.Abstractions
{
    public interface IMetric
    {
        string Name { get; }
        string Description { get; }
    }
}