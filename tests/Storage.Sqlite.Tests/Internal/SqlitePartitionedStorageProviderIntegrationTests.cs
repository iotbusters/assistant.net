﻿using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Internal
{
    public class SqlitePartitionedStorageProviderIntegrationTests
    {
        [Test]
        public async Task Add_returnsAddedValue_noKey()
        {
            var value = await Storage.Add(TestKey, TestValue("added", version: 1));

            value.Should().Be(1);
        }

        [Test]
        public async Task Add_returnsExistingValue_keyExists()
        {
            await Storage.Add(TestKey, TestValue("added-1", version: 1));

            var value = await Storage.Add(TestKey, TestValue("added-2", version: 2));

            value.Should().Be(2);
        }

        [Test]
        public async Task TryGet_returnsNone_noKey()
        {
            var value = await Storage.TryGet(TestKey, index: 1);

            value.Should().Be((Option<ValueRecord>)Option.None);
        }

        [Test]
        public async Task TryGet_returnsExistingValue_keyExits()
        {
            await Storage.Add(TestKey, TestValue("added"));

            var value = await Storage.TryGet(TestKey, index: 1);

            value.Should().BeEquivalentTo(new {Value = TestValue("added")}, o => o.ComparingByMembers<ValueRecord>());
        }

        [Test]
        public async Task GetKeys_returnsKeys()
        {
            await Storage.Add(TestKey, TestValue("value"));

            var value = Storage.GetKeys().ToArray();

            value.Should().BeEquivalentTo(new[] {TestKey});
        }

        [Test]
        public async Task TryRemove_returnsZero_noKey()
        {
            var count = await Storage.TryRemove(TestKey, upToIndex: 10);

            count.Should().Be(0);
        }

        [Test]
        public async Task TryRemove_returnsOne_keyExists()
        {
            await Storage.Add(TestKey, TestValue("value"));

            var count = await Storage.TryRemove(TestKey, upToIndex: 10);

            count.Should().Be(1);
        }

        [Test]
        public async Task TryRemove_returnsOne_twoKeysExist()
        {
            await Storage.Add(TestKey, TestValue("value-1", version: 1));
            await Storage.Add(TestKey, TestValue("value-2", version: 2));

            var count1 = await Storage.TryRemove(TestKey, upToIndex: 1);
            count1.Should().Be(1);

            var value1 = await Storage.TryGet(TestKey, index: 1);
            value1.Should().Be((Option<ValueRecord>)Option.None);

            var value2 = await Storage.TryGet(TestKey, index: 2);
            value2.Should().BeEquivalentTo(new {Value = new {Type = "value-2"}});

            var count2 = await Storage.TryRemove(TestKey, upToIndex: 2);
            count2.Should().Be(1);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            var connectionString = "Data Source=:memory:";
            Provider = new ServiceCollection()
                .AddStorage(b => b
                    .UseSqlite(connectionString)
                    .AddSqlitePartitioned<TestKey, TestValue>())
                .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
                .AddSystemClock(_ => TestDate)
                .BuildServiceProvider();
            DbContext = await Provider.GetRequiredService<IDbContextFactory<HistoricalStorageDbContext>>().CreateDbContextAsync();
            await DbContext.Database.EnsureCreatedAsync(CancellationToken);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DbContext?.Database.EnsureDeleted();
            Provider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            DbContext?.Keys.RemoveRange(DbContext.Keys);
            DbContext?.SaveChanges();
        }

        private static CancellationToken CancellationToken => new CancellationTokenSource(500).Token;
        private ValueRecord TestValue(string type, int version = 1) => new(Type: type, Content: Array.Empty<byte>(), Audit(version));
        private Audit Audit(int version = 1) => new(TestCorrelationId, TestUser, TestDate, version);
        private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", content: Array.Empty<byte>());
        private string TestCorrelationId { get; } = Guid.NewGuid().ToString();
        private string TestUser { get; } = Guid.NewGuid().ToString();
        private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
        private ServiceProvider? Provider { get; set; }
        private HistoricalStorageDbContext? DbContext { get; set; }
        private IPartitionedStorageProvider<TestValue> Storage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IPartitionedStorageProvider<TestValue>>();
    }
}