using System.Linq;
using Microsoft.AspNetCore.Routing.Patterns;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Web.Server.Tests.Mocks;

namespace Assistant.Net.Messaging.Web.Server.Tests
{
    public class EndpointRouteBuilderExtensionsTests
    {
        [Test]
        public void MapRemoteCommandHandling_registersRoutePatternAndRequestDelegate()
        {
            var builder = new TestEndpointRouteBuilder();

            builder.MapRemoteCommandHandling();

            builder.DataSources.Should().BeEquivalentTo(new
            {
                Endpoints = new[] { new { RoutePattern = RoutePatternFactory.Parse("/command") } }
            });
            builder.DataSources.Single().Endpoints.Single().RequestDelegate
                .Should().NotBeNull();
        }
    }
}