using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Mongo.Tests.Mocks;

public class TestServerAvailabilityService : IServerAvailabilityService
{
    public Task Register(string name, TimeSpan timeToLive, CancellationToken token) => Task.CompletedTask;

    public Task Unregister(string name, CancellationToken token) => Task.CompletedTask;
}
