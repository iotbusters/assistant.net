using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests
{
    public class HealthCheckServiceTests
    {
        [Test]
        public async Task CheckHealthAsync_returnsHealthy_twoSameNamedMongoOptions()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureMongoOptions("1", o => o.Connection(ValidConnectionString).Database("test"))
                .ConfigureMongoOptions("2", o => o.Connection(ValidConnectionString).Database("test"))
                .AddHealthChecks()
                .AddMongo("1", timeout)
                .AddMongo("2", timeout)
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            report.Should().BeEquivalentTo(new
            {
                Status = HealthStatus.Healthy,
                Entries = new[]
                {
                    new {Key = "storage-1", Value = new {Status = HealthStatus.Healthy}},
                    new {Key = "storage-2", Value = new {Status = HealthStatus.Healthy}}
                }
            });
        }

        [Test]
        public async Task CheckHealthAsync_returnsHealthy_twoDifferentlyNamedMongoOptions()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureMongoOptions("1", o => o.Connection(ValidConnectionString).Database("test1"))
                .ConfigureMongoOptions("2", o => o.Connection(ValidConnectionString).Database("test2"))
                .AddHealthChecks()
                .AddMongo("1", timeout)
                .AddMongo("2", timeout)
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            report.Should().BeEquivalentTo(new
            {
                Status = HealthStatus.Healthy,
                Entries = new[]
                {
                    new {Key = "storage-1", Value = new {Status = HealthStatus.Healthy}},
                    new {Key = "storage-2", Value = new {Status = HealthStatus.Healthy}}
                }
            });
        }

        [Test]
        public async Task CheckHealthAsync_returnsUnhealthy_oneOfNamedMongoOptionsIsInvalid()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureMongoOptions("1", o => o.Connection(ValidConnectionString).Database("test"))
                .ConfigureMongoOptions("2", o => o.Connection(InvalidConnectionString).Database("test"))
                .AddHealthChecks()
                .AddMongo("1", timeout)
                .AddMongo("2", timeout)
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            report.Should().BeEquivalentTo(new
            {
                Status = HealthStatus.Unhealthy,
                Entries = new[]
                {
                    new {Key = "storage-1", Value = new {Status = HealthStatus.Healthy}},
                    new {Key = "storage-2", Value = new {Status = HealthStatus.Unhealthy}}
                }
            });
        }

        private const string ValidConnectionString = "mongodb://127.0.0.1:27017";
        private const string InvalidConnectionString = "mongodb://invalid";
        private static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureMongoOptions(o => o.Connection(ValidConnectionString).Database("test"))
                .AddHealthChecks()
                .AddMongo(timeout: TimeSpan.FromSeconds(5))
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            if (report.Status != HealthStatus.Healthy)
                Assert.Ignore($"The tests require mongodb instance at {ValidConnectionString}.");
        }
    }
}
