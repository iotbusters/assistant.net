namespace Assistant.Net.Metrics.Abstractions
{
    public interface IGaugeMetric : IMetric
    {
        void Set(int value);
    }
}