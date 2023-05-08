using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Sqlite.Tests.Mocks;

public class TestServerActivityService : IServerActivityService
{
    public bool IsActivationRequested => throw new NotImplementedException();

    public TimeSpan DelayTime { get; set; } = TimeSpan.Zero;

    public void Activate() { }

    public void Inactivate() { }

    public Task DelayInactive(CancellationToken token) => Task.Delay(DelayTime, token);
}
