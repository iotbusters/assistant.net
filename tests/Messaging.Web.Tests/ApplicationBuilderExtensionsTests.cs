using Assistant.Net.Messaging.Web.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;

namespace Assistant.Net.Messaging.Web.Tests;

public class ApplicationBuilderExtensionsTests
{
    [Test]
    public void MapRemoteMessageHandling_registersRoutePatternAndRequestDelegate()
    {
        var builder = new TestApplicationBuilder();

        builder.UseWebMessageHandling();

        builder.Count.Should().Be(6,
            @"Expected middlewares are:
1. EndpointRoutingMiddleware,
2. EndpointMiddleware,
3. DiagnosticMiddleware,
4. ExceptionHandlingMiddleware,
5. MessageHandlingMiddleware,
6. HealthCheckMiddleware");
    }
}
