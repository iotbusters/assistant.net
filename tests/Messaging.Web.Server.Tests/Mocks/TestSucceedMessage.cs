using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks;

public record TestSucceedMessage(TestResponse Payload) : IMessage<TestResponse>;
