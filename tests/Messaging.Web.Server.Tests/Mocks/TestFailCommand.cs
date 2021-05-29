using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    public record TestFailCommand(string? AssemblyQualifiedExceptionTypeName) : ICommand;
}