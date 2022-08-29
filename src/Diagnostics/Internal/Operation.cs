using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;
using System;
using System.Diagnostics;

namespace Assistant.Net.Diagnostics.Internal;

/// <summary>
///     Tracking operation implementation.
/// </summary>
internal sealed class Operation : IOperation, IDisposable
{
    public const string DefaultCompleteMessage = "Operation has successfully completed.";
    public const string DefaultFailMessage = "Operation has failed.";
    public const string LostOperationMessage = "Operation wasn't properly stopped or lost.";

    private readonly DiagnosticFactory factory;
    private readonly Activity activity;

    private bool isStopped;

    public Operation(string name, DiagnosticFactory factory)
    {
        this.factory = factory;
        this.activity = new Activity(name)
            .SetStartTime(factory.Clock.UtcNow.UtcDateTime)
            .Start();

        // the following can be called after operation is started only.
        var activityId = activity.Id ?? throw new InvalidOperationException("Activity wasn't started.");
        var correlationId = factory.Context.CorrelationId ?? throw new InvalidOperationException("CorrelationId wasn't setup properly.");
        this.activity.AddCorrelationId(correlationId);
        this.factory.Operations.Add(activityId, this);

        WriteOperationStarted();
    }

    public IOperation AddData(string name, string value)
    {
        activity.SetTag(name, value);
        return this;
    }

    void IOperation.Complete(string? message) =>
        Stop(OperationStatus.Complete, message ?? DefaultCompleteMessage);

    void IOperation.Fail(string? message) =>
        Stop(OperationStatus.Failed, message ?? DefaultFailMessage);

    void IDisposable.Dispose() =>
        Stop(OperationStatus.Incomplete, LostOperationMessage);

    /// <exception cref="ArgumentException"/>
    private void Stop(string status, string message)
    {
        if (isStopped) return;
        isStopped = true;

        activity
            .AddOperationStatus(status)
            .AddMessage(message)
            .SetEndTime(factory.Clock.UtcNow.UtcDateTime)
            .Stop();
        activity.Dispose();

        WriteOperationStopped(status, message);

        factory.Operations.Remove(activity.Id!);
    }

    /// <exception cref="ArgumentException"/>
    private void WriteOperationStarted()
    {
        var data = new OperationStartedData
        {
            CorrelationId = activity.GetCorrelationId(),
            ParentCorrelationIds = activity.GetParentCorrelationIds(),
            Metadata = activity.GetCustomMetadata()
        };

        factory.EventSource.WriteOperationStarted(activity.OperationName, ref data);
    }

    /// <exception cref="ArgumentException"/>
    private void WriteOperationStopped(string status, string message)
    {
        var data = new OperationStoppedData
        {
            CorrelationId = activity.GetCorrelationId(),
            ParentCorrelationIds = activity.GetParentCorrelationIds(),
            Duration = activity.Duration,
            Status = status,
            Message = message,
            Metadata = activity.GetCustomMetadata()
        };

        factory.EventSource.WriteOperationStopped(activity.OperationName, ref data);

    }
}
