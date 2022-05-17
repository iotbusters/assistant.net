using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Web.Server.Tests.Mocks;

namespace Assistant.Net.Messaging.Web.Server.Tests;

public class ApplicationBuilderExtensionsTests
{
    [Test]
    public void MapRemoteMessageHandling_registersRoutePatternAndRequestDelegate()
    {
        var builder = new TestApplicationBuilder();

        builder.UseRemoteWebMessageHandler();

        builder.Count.Should().Be(3);// it would be difficult to resolve specific middlewares.
    }
}
