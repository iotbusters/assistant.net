using Assistant.Net.Diagnostics.EventSources;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.Tests.Mocks
{
    public class TestOperationEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name != EventNames.OperationEventName)
                return;

            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData) =>
            EventPayloads.Add(eventData);

        public List<EventWrittenEventArgs> EventPayloads { get; } = new();
    }
}
