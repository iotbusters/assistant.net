using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks.Stubs
{
    public record TestCommand1(int Scenario) : ICommand<TestResponse>;
}