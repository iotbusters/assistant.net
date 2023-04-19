using Assistant.Net.Storage.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Linq;

namespace Assistant.Net.Storage.Mongo.Tests
{
    public class HealthCheckBuilderExtensionsTests
    {
        [Test]
        public void AddMongo_registersHealthCheck_defaultConfiguration()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .AddHealthChecks()
                .AddMongo()
                .Services
                .BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().BeEquivalentTo(new[] {new {Name = "storage-"}});

            var registration = options.Registrations.Single();
            registration.Factory(provider).GetType().Name.Should().Be("MongoOptionsHealthCheck");
        }

        [Test]
        public void AddMongo_registersHealthCheck_namedConfiguration()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .AddHealthChecks()
                .AddMongo("test")
                .Services
                .BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().BeEquivalentTo(new[] {new {Name = "storage-test"}});

            var registration = options.Registrations.Single();
            registration.Factory(provider).GetType().Name.Should().Be("MongoOptionsHealthCheck");
        }
    }
}
