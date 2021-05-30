using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Web.Server.Tests.Mocks;

namespace Assistant.Net.Messaging.Web.Server.Tests
{
    public class ApplicationBuilderExtensionsTests
    {
        [Test]
        public void MapRemoteCommandHandling_registersRoutePatternAndRequestDelegate()
        {
            var builder = new TestApplicationBuilder();

            builder.UseRemoteWebCommandHandler();

            builder.Count.Should().Be(3);// it would be difficult to resolve specific middlewares.
        }
    }
}