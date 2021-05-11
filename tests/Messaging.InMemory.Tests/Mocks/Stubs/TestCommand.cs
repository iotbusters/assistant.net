using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.InMemory.Tests.Mocks.Stubs
{
    public record TestCommand(int Scenario) : ICommand<TestResponse>;
}