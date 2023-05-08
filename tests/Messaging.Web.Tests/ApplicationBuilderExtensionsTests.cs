using Assistant.Net.Messaging.Web.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;

namespace Assistant.Net.Messaging.Web.Tests;

public class ApplicationBuilderExtensionsTests
{
    [Test]
    public void MapRemoteMessageHandling_registersRoutePatternAndRequestDelegate_defaultConfiguration()
    {
        var builder = new TestApplicationBuilder();

        builder.UseWebMessageHandling();

        builder.Count.Should().Be(7,
            @"Expected middlewares are:
1. EndpointRoutingMiddleware,
2. EndpointMiddleware,
3. DiagnosticMiddleware,
4. ExceptionHandlingMiddleware,
5. MessageHandlingMiddleware(""""),
6. MessageHandlingMiddleware(),
7. HealthCheckMiddleware");
    }

    [Test]
    public void MapRemoteMessageHandling_registersRoutePatternAndRequestDelegate_multipleNamedConfigurations()
    {
        var builder = new TestApplicationBuilder();

        builder.UseWebMessageHandling("1", "2");

        builder.Count.Should().Be(8,
            @"Expected middlewares are:
1. EndpointRoutingMiddleware,
2. EndpointMiddleware,
3. DiagnosticMiddleware,
4. ExceptionHandlingMiddleware,
5. MessageHandlingMiddleware(""1""),
6. MessageHandlingMiddleware(""2""),
7. MessageHandlingMiddleware(),
8. HealthCheckMiddleware");
    }
}
