using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks
{
    public record TestCommand2(Exception? Exception) : ICommand;
}