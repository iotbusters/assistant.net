using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests.Mocks.Stubs
{
    public record TestCommand1(Exception? Exception) : ICommand<TestResponse>;
}