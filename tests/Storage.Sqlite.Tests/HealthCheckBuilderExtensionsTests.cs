using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Linq;

namespace Assistant.Net.Storage.Sqlite.Tests
{
    public class HealthCheckBuilderExtensionsTests
    {
        [Test]
        public void AddSqlite_registersHealthCheck_defaultConfiguration()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .AddHealthChecks()
                .AddSqlite()
                .Services
                .BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().BeEquivalentTo(new[] {new {Name = "storage-"}});

            var registration = options.Registrations.Single();
            registration.Factory(provider).GetType().Name.Should().Be("SqliteOptionsHealthCheck");
        }

        [Test]
        public void AddSqlite_registersHealthCheck_namedConfiguration()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .AddHealthChecks()
                .AddSqlite("test")
                .Services
                .BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().BeEquivalentTo(new[] {new {Name = "storage-test"}});

            var registration = options.Registrations.Single();
            registration.Factory(provider).GetType().Name.Should().Be("SqliteOptionsHealthCheck");
        }
    }
}
