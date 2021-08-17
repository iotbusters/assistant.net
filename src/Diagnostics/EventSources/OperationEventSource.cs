using System;
using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    /// <summary>
    ///     Tracking operation oriented event source implementation.
    /// </summary>
    internal sealed class OperationEventSource : EventSource
    {
        public static OperationEventSource Instance { get; } = new OperationEventSource();

        private EventSourceOptions startOptions = new() { Level = EventLevel.LogAlways, Opcode = EventOpcode.Start, ActivityOptions = EventActivityOptions.Recursive | EventActivityOptions.Detachable};
        private EventSourceOptions stopOptions = new() { Level = EventLevel.LogAlways, Opcode = EventOpcode.Stop, ActivityOptions = EventActivityOptions.Recursive | EventActivityOptions.Detachable };
        private Guid noId;

        private OperationEventSource() : base(EventNames.OperationEventName) { }

        internal void WriteOperationStarted(string eventName, ref OperationStartedData data)
        {
            if (!IsEnabled()) return;

            Write(eventName, ref startOptions, ref noId, ref noId, ref data);
        }

        internal void WriteOperationStopped(string eventName, ref OperationStoppedData data)
        {
            if (!IsEnabled()) return;

            Write(eventName, ref stopOptions, ref noId, ref noId, ref data);
        }
    }
}