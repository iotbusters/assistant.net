using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Mongo.Tests.Mocks;

public record TestSuccessFailureMessage(string? AssemblyQualifiedExceptionType) : IMessage;
