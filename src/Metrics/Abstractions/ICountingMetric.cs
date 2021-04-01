namespace Assistant.Net.Metrics.Abstractions
{
    public interface ICountingMetric : IMetric
    {
        void Increment();
    }
}