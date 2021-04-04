namespace Assistant.Net.Metrics.Abstractions
{
    public interface ICounterMetric : IMetric
    {
        void Increment();
    }
}