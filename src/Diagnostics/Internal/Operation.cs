using System;
using System.Diagnostics;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;

namespace Assistant.Net.Diagnostics.Internal
{
    internal sealed class Operation : IOperation, IDisposable
    {
        public const string DefaultCompleteMessage = "Operation has successfully completed.";
        public const string DefaultFailMessage = "Operation has failed.";
        public const string LostOperationMessage = "Operation wasn't properly stopped or lost.";

        private readonly DiagnosticsFactory factory;
        private readonly Activity activity;

        private bool isStopped;

        public Operation(string name, DiagnosticsFactory factory)
        {
            this.factory = factory;
            this.activity = new Activity(name)
                .SetStartTime(factory.Clock.UtcNow.UtcDateTime)
                .Start();

            // the following can be called after operation is started only.
            activity.AddCorrelationId(factory.Context.CorrelationId);
            factory.Operations.Add(activity.Id!, this);

            WriteOperationStarted();
        }

        void IOperation.Complete(string? message) =>
            Stop(OperationStatus.Completed, message ?? DefaultCompleteMessage);

        void IOperation.Fail(string? message) =>
            Stop(OperationStatus.Failed, message ?? DefaultFailMessage);

        void IDisposable.Dispose() =>
            Stop(OperationStatus.Incompleted, LostOperationMessage);

        // todo: consider failing with user-data.
        private void Stop(string status, string message)
        {
            if (isStopped) return;
            isStopped = true;

            activity
                .AddOperationStatus(status)
                .AddMessage(message)
                .SetEndTime(factory.Clock.UtcNow.UtcDateTime)
                .Stop();

            WriteOperationStopped(status, message);

            factory.Operations.Remove(activity.Id!);
        }

        private void WriteOperationStarted()
        {
            var data = new OperationStartedData
            {
                Metadata = activity.GetCustomMetadata()
            };
            var correlationId = factory.Context.CorrelationId;

            factory.EventSource.WriteOperationStarted(
                activity.OperationName,
                ref data,
                ref correlationId);
        }

        private void WriteOperationStopped(string status, string message)
        {
            var data = new OperationStoppedData
            {
                Duration = activity.Duration,
                Status = status,
                Message = message,
                Metadata = activity.GetCustomMetadata()
            };
            var correlationId = factory.Context.CorrelationId;

            factory.EventSource.WriteOperationStopped(
                activity.OperationName,
                ref data,
                ref correlationId);

        }
    }
}