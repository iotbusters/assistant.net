using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Options;
using System;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestNamedOptions : INamedOptions<MessagingClientOptions>
{
    private readonly Func<MessagingClientOptions> factory;

    public TestNamedOptions(MessagingClientOptions value) : this(() => value) { }

    public TestNamedOptions(Func<MessagingClientOptions> factory) =>
        this.factory = factory;

    public MessagingClientOptions Value => factory();
}
