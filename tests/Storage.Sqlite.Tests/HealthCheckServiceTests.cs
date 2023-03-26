using Assistant.Net.Storage.Options;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests
{
    public class HealthCheckServiceTests
    {
        [Test]
        public async Task CheckHealthAsync_returnsHealthy_oneNamedSqliteOptions()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureSqliteOptions(o => o.Connection(ValidConnectionString))
                .AddHealthChecks()
                .AddSqlite(timeout)
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            report.Should().BeEquivalentTo(new
            {
                Status = HealthStatus.Healthy,
                Entries = new[] {new {Key = nameof(SqliteOptions), Value = new {Status = HealthStatus.Healthy}}}
            });
        }

        [Test]
        public async Task CheckHealthAsync_returnsHealthy_twoSameNamedSqliteOptions()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureSqliteOptions("1", o => o.Connection(ValidConnectionString))
                .ConfigureSqliteOptions("2", o => o.Connection(ValidConnectionString))
                .AddHealthChecks()
                .AddSqlite(timeout)
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            report.Should().BeEquivalentTo(new
            {
                Status = HealthStatus.Healthy,
                Entries = new[] {new {Key = nameof(SqliteOptions), Value = new {Status = HealthStatus.Healthy}}}
            });
        }

        [Test]
        public async Task CheckHealthAsync_returnsHealthy_twoDifferentlyNamedSqliteOptions()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureSqliteOptions("1", o => o.Connection(ValidConnectionString))
                .ConfigureSqliteOptions("2", o => o.Connection(ValidConnectionString))
                .AddHealthChecks()
                .AddSqlite(timeout)
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            report.Should().BeEquivalentTo(new
            {
                Status = HealthStatus.Healthy,
                Entries = new[] {new {Key = nameof(SqliteOptions), Value = new {Status = HealthStatus.Healthy}}}
            });
        }

        [Test]
        public async Task CheckHealthAsync_returnsUnhealthy_oneOfNamedSqliteOptionsIsInvalid()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .ConfigureSqliteOptions("1", o => o.Connection(ValidConnectionString))
                .ConfigureSqliteOptions("2", o => o.Connection(InvalidConnectionString))
                .AddHealthChecks()
                .AddSqlite(timeout)
                .Services
                .BuildServiceProvider();

            var service = provider.GetRequiredService<HealthCheckService>();
            var report = await service.CheckHealthAsync();

            report.Should().BeEquivalentTo(new
            {
                Status = HealthStatus.Unhealthy,
                Entries = new[] {new {Key = nameof(SqliteOptions), Value = new {Status = HealthStatus.Unhealthy}}}
            });
        }

        [OneTimeSetUp]
        public async Task OnetimeSetup() =>
            await MasterConnection.OpenAsync(new CancellationTokenSource(5000).Token);

        private SqliteConnection MasterConnection { get; } = new(ValidConnectionString);

        private const string ValidConnectionString = "Data Source=test;Mode=Memory;Cache=Shared";
        private const string InvalidConnectionString = "Data Source=file.db;Password=unsupported;";
        private static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);
    }
}
