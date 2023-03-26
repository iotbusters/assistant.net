using Assistant.Net.Storage.Options;
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
        public void AddMongo_registersHealthCheck()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureSqliteOptions(":memory:")
                .AddHealthChecks()
                .AddSqlite()
                .Services
                .BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().BeEquivalentTo(new[] {new {Name = nameof(SqliteOptions)}});

            var registration = options.Registrations.Single();
            registration.Factory(provider).GetType().Name.Should().Be("SqliteOptionsHealthCheck");
        }
    }
}
