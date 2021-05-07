using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Assistant.Net.Diagnostics.EventSources;

namespace Assistant.Net.Diagnostics.Tests.Mocks
{
    public class TestOperationEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name != EventNames.EventName)
                return;

            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData) =>
            EventPayloads.Add(eventData);

        public List<EventWrittenEventArgs> EventPayloads { get; } = new List<EventWrittenEventArgs>();
    }
}