﻿using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Internal;

public class SqlitePartitionedStorageProviderIntegrationTests
{
    [Test]
    public async Task Add_returnsAddedValue_noKey()
    {
        var value1 = TestValue();
        var value = await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(value1),
            updateFactory: (_, _) => throw new NotImplementedException());

        value.Should().Be(value1);
    }

    [Test]
    public async Task Add_returnsExistingValue_keyExists()
    {
        var value1 = TestValue();
        var value20 = TestValue(version: 20);
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(value1),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.Add(
            TestKey,
            addFactory: _ => throw new NotImplementedException(),
            updateFactory: (_, _) => Task.FromResult(value20));

        value.Should().Be(value20);
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
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue()),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.TryGet(TestKey, index: 1);

        value.Should().BeEquivalentTo(new {Value = TestValue()}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue()),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.GetKeys(_ => true).ToArrayAsync();

        value.Should().BeEquivalentTo(new[] {TestKey});
    }

    [Test]
    public async Task TryRemove_returnsZero_noKey()
    {
        var value = await Storage.TryRemove(TestKey, upToIndex: 10);

        value.Should().Be(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsOne_keyExists()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue()),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.TryRemove(TestKey, upToIndex: 10);

        value.Should().BeEquivalentTo(TestValue().AsOption());
    }

    [Test]
    public async Task TryRemove_returnsOne_twoKeysExist()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue(version: 1)),
            updateFactory: (_, _) => throw new NotImplementedException());
        await Storage.Add(
            TestKey,
            addFactory: _ => throw new NotImplementedException(),
            updateFactory: (_, _) => Task.FromResult(TestValue(version: 2)));

        var value = await Storage.TryRemove(TestKey, upToIndex: 1);
        value.Should().BeEquivalentTo(TestValue(version: 1).AsOption());

        var value1 = await Storage.TryGet(TestKey, index: 1);
        value1.Should().Be(new None<ValueRecord>());

        var value2 = await Storage.TryGet(TestKey, index: 2);
        value2.Should().BeEquivalentTo(TestValue(version: 2).AsOption());

        var value3 = await Storage.TryRemove(TestKey, upToIndex: 2);
        value3.Should().BeEquivalentTo(TestValue(version: 2).AsOption());
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .Add<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestValue>>();
        var storage2 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.Add(key, new(true));
        var value = await storage2.TryGet(key, 1);

        value.Should().Be(Option.Some(new TestValue(true)));
    }

    [Test]
    public async Task TryGet_returnsNone_FromStorageOfAnotherValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .Add<TestKey, TestBase>()
                .Add<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.Add(key, new TestValue(true));
        var value = await storage2.TryGet(key, 1);

        value.Should().Be((Option<TestValue>)Option.None);
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageUsedAdding()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .Add<TestKey, TestBase>()
                .Add<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.Add(key, new TestValue(true));
        await storage2.Add(key, new(false));
        var value1 = await storage1.TryGet(key, 1);
        var value2 = await storage2.TryGet(key, 1);
        var value3 = await storage1.TryGet(key, 2);
        var value4 = await storage2.TryGet(key, 3);

        value1.Should().Be(Option.Some<TestBase>(new TestValue(true)));
        value2.Should().Be(Option.Some(new TestValue(false)));
        value3.Should().Be((Option<TestBase>)Option.None);
        value4.Should().Be((Option<TestValue>)Option.None);
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .Add<TestKey, TestValue>())
            .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
            .AddSystemClock(_ => TestDate)
            .BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider?.Dispose();

    [SetUp]
    public async Task Cleanup()
    {
        var dbContext = await Provider!.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync(CancellationToken);
        dbContext.HistoricalKeys.RemoveRange(dbContext.HistoricalKeys);
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private static CancellationToken CancellationToken => new CancellationTokenSource(100).Token;
    private ValueRecord TestValue(int version = 1) => new(Content: Array.Empty<byte>(), Audit(version));
    private Audit Audit(int version = 1) => new(version) {CorrelationId = TestCorrelationId, User = TestUser, Created = TestDate};
    private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", valueType: nameof(Mocks.TestValue), content: Array.Empty<byte>());
    private string TestCorrelationId { get; } = Guid.NewGuid().ToString();
    private string TestUser { get; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
    private ServiceProvider? Provider { get; set; }

    private IPartitionedStorageProvider<TestValue> Storage => (IPartitionedStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value
            .PartitionedStorageProviderFactory!.Create(Provider!, typeof(TestValue));
}
