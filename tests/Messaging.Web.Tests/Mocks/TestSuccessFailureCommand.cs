using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Tests.Mocks
{
    public record TestSuccessFailureMessage(string? AssemblyQualifiedExceptionType) : IMessage;
}