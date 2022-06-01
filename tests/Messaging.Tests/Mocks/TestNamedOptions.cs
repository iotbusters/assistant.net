using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestNamedOptions : INamedOptions<MessagingClientOptions>
{
    public MessagingClientOptions Value { get; set; } = null!;
}
