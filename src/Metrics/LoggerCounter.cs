using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Tracing;
using Assistant.Net.Abstractions;

namespace Assistant.Net.Metrics
{
    public sealed class RequestCounterSource : EventSource
    {
        // define the singleton instance of the event source
        public static RequestCounterSource Log = new RequestCounterSource(new TestSystemClock(() => DateTimeOffset.UtcNow));

        private readonly ISystemClock clock;

        private readonly EventCounter timeCounter;
        private readonly IncrementingEventCounter requestCounter;

        private RequestCounterSource(ISystemClock clock) : base("Assistant-Net-Diagnostic-Request")
        {
            this.clock = clock;
            timeCounter = new EventCounter("api-request-time", this)
            {
                DisplayName = "Request Processing Time",
                DisplayUnits = "ms"
            };
            requestCounter = new IncrementingEventCounter("api-request-count", this)
            {
                DisplayName = "API Request Count"
            };
        }

        public IDisposable StartRequest()
        {
            using var activity = new Activity("Request")
                .SetTag("key1","value1")
                .SetTag("key2","value2")
                .SetStartTime(clock.UtcNow.DateTime)
                .Start();

            activity.AddEvent(new ActivityEvent("event 1", DateTimeOffset.UtcNow, new ActivityTagsCollection {{"key3", "value3"}}));

            return new DisposableAction(() =>
            {
                requestCounter.Increment();
                Write("event N", 1);

                activity
                    .SetEndTime(clock.UtcNow.DateTime)
                    .Dispose();
            });
        }
    }

    internal class DisposableAction : IDisposable
    {
        private readonly Action dispose;

        public DisposableAction(Action dispose) => this.dispose = dispose;

        public void Dispose() => dispose();
    }
    internal class TestSystemClock : ISystemClock
    {
        private readonly Func<DateTimeOffset> getTime;

        public TestSystemClock(Func<DateTimeOffset> getTime)
        {
            this.getTime = getTime;
        }

        public DateTimeOffset UtcNow => getTime();
    }
    public class Program
    {
        public void Main()
        {


            //IMetric metric;
            //var customMetric = metric.Name("custom-metric").Label((start: 1, end: 2));
            //using (customMetric.Counter().Stopwatch().ScopedSetter(() => (1, 2)))
            //{
            //}
        }
    }

    //public class Metric : IMetric
    //{
    //    private readonly ILoggerFactory factory;

    //    public Metric(ILoggerFactory factory) =>
    //        this.factory = factory;

    //    public INamedMetric Name(string name) => new MetricBuilder(name, factory.CreateLogger("Assistant.Net.Metrics"));
    //}

    //public class MetricBuilder : INamedMetric, IScopedMetric
    //{
    //    private readonly string name;
    //    private readonly ILogger logger;
    //    private IDisposable? labeledScope;
    //    private Action registrations = delegate { };

    //    public MetricBuilder(string name, ILogger logger)
    //    {
    //        this.name = name;
    //        this.logger = logger;
    //    }

    //    public IScopedMetric Counter()
    //    {
    //        registrations += () => logger.Log(LogLevel.Information, new EventId(1, name), "");
    //        return this;
    //    }

    //    public IScopedMetric Stopwatch()
    //    {
    //        var startTime = DateTimeOffset.UtcNow;
    //        registrations += () =>
    //        {
    //            var endTime = DateTimeOffset.UtcNow;
    //            logger.Log(LogLevel.Information, new EventId(2, name), "{Start}, {End}, {Duration}", startTime, endTime, endTime - startTime);
    //        };
    //        return this;
    //    }

    //    public IScopedMetric ScopedSetter<T>(Func<T> set)
    //    {
    //        registrations += () => logger.Log(LogLevel.Information, new EventId(3, name), "{@Value}", set());
    //        return this;
    //    }

    //    public IManualMetric<TValue> Manual<TValue>() => new ManualMetric<TValue>(name, logger);

    //    public ILabeledMetric Label<T>(T label) where T : struct
    //    {
    //        labeledScope = logger.BeginScope(label);
    //        return this;
    //    }

    //    public void Complete()
    //    {
    //        labeledScope?.Dispose();
    //        registrations();
    //    }
    //}

    //public class ManualMetric<TValue> : IManualMetric<TValue>
    //{
    //    private readonly string name;
    //    private readonly ILogger logger;

    //    public ManualMetric(string name, ILogger logger)
    //    {
    //        this.name = name;
    //        this.logger = logger;
    //    }

    //    public void Set(TValue value)
    //    {
    //        var message = typeof(TValue).IsPrimitive ? "{Value}" : "{@Value}";
    //        logger.Log(LogLevel.Information, new EventId(3, name), message, value);
    //    }
    //}

    //public interface IMetric
    //{
    //    INamedMetric Name(string name);
    //}

    //public interface INamedMetric : ILabeledMetric
    //{
    //    ILabeledMetric Label<T>(T labels) where T: struct;
    //}

    //public interface ILabeledMetric : IMultiMetric
    //{
    //    IManualMetric<T> Manual<T>();
    //}

    //public interface IMultiMetric
    //{
    //    IScopedMetric Counter();
    //    IScopedMetric Stopwatch();
    //    IScopedMetric ScopedSetter<T>(Func<T> set);
    //}

    //public interface IScopedMetric : IMultiMetric, IDisposable
    //{
    //    void Complete();
    //    void IDisposable.Dispose() => Complete();
    //}

    //public interface IManualMetric<TValue>
    //{
    //    void Set(TValue value);
    //}

    //public static class Extensions
    //{
    //}
}