namespace Assistant.Net.Metrics.Abstractions
{
    public interface IHistogramMetric<T> : IMetric where T : struct
    {
        void Set(T value);
    }
}