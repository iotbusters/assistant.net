using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestScenarioMessageHandler : IMessageHandler<TestMessage, TestResponse>
{
    private readonly Func<TestMessage, Task<TestResponse>> handler;

    public TestScenarioMessageHandler() : this(DefaultBehavior) { }

    public TestScenarioMessageHandler(Exception exception) : this(_ => Task.FromException<TestResponse>(exception)) { }

    public TestScenarioMessageHandler(TestResponse response) : this(_ => Task.FromResult(response)) { }

    public TestScenarioMessageHandler(Func<TestMessage, Task<TestResponse>> handler) =>
        this.handler = handler;

    public Task<TestResponse> Handle(TestMessage message, CancellationToken token) => handler(message);

    private static Task<TestResponse> DefaultBehavior(TestMessage message) => message.Scenario switch
    {
        0 => Task.FromResult(new TestResponse(false)),
        _ => Task.FromException<TestResponse>(new InvalidOperationException("test"))
    };
}
