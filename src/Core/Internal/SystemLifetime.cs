using System.Threading;
using Assistant.Net.Abstractions;
using System;

namespace Assistant.Net.Internal;

/// <summary>
///     Default system lifetime implementation.
/// </summary>
internal class SystemLifetime : ISystemLifetime, IDisposable
{
    private readonly CancellationTokenSource stopping;

    public SystemLifetime(CancellationToken stopping)
    {
        this.stopping = CancellationTokenSource.CreateLinkedTokenSource(stopping);
    }

    public CancellationToken Stopping => stopping.Token;

    public void RequestStopping(TimeSpan? after) => stopping.CancelAfter(after ?? TimeSpan.Zero);

    public void Dispose() => stopping.Dispose();
}