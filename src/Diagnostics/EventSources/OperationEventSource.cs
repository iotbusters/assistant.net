using System;
using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    internal sealed class OperationEventSource : EventSource
    {
        public static OperationEventSource Instance { get; } = new OperationEventSource();

        private EventSourceOptions startOptions = new EventSourceOptions { Level = EventLevel.LogAlways, Opcode = EventOpcode.Start, ActivityOptions = EventActivityOptions.Recursive | EventActivityOptions.Detachable};
        private EventSourceOptions stopOptions = new EventSourceOptions { Level = EventLevel.LogAlways, Opcode = EventOpcode.Stop, ActivityOptions = EventActivityOptions.Recursive | EventActivityOptions.Detachable };
        private Guid noId;

        private OperationEventSource() : base(EventNames.EventName) { }

        internal void WriteOperationStarted(string eventName, ref OperationStartedData data, ref Guid correlationId)
        {
            if (!IsEnabled()) return;

            Write(eventName, ref startOptions, ref noId, ref correlationId, ref data);
        }

        internal void WriteOperationStopped(string eventName, ref OperationStoppedData data, ref Guid correlationId)
        {
            if (!IsEnabled()) return;

            Write(eventName, ref stopOptions, ref noId, ref correlationId, ref data);
        }
    }
}