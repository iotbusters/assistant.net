using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Integration.Tests.Mocks
{
    public record TestSuccessFailureMessage(string? AssemblyQualifiedExceptionType) : IMessage;
}