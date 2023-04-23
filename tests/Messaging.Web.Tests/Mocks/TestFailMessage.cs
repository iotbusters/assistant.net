using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Tests.Mocks;

public record TestFailMessage(string? AssemblyQualifiedExceptionTypeName) : IMessage;
