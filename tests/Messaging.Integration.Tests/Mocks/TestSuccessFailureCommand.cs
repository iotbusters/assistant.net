using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Integration.Tests.Mocks
{
    public record TestSuccessFailureCommand(string? AssemblyQualifiedExceptionType) : ICommand;
}