﻿using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Internal;

public class SqlitePartitionedStorageIntegrationTests
{
    [Test]
    public async Task Add_returnsAddedValue_noKey()
    {
        var value = await Storage.Add(new(true), new TestValue(true));

        value.Should().Be(1);
    }

    [Test]
    public async Task Add_returnsExistingValue_keyExists()
    {
        await Storage.Add(new(true), new TestValue(true));

        var value = await Storage.Add(new(true), new TestValue(false));

        value.Should().Be(2);
    }

    [Test]
    public async Task Add_returnsAddedPartitionValue_noKey()
    {
        var value = await Storage.Add(new(true), new(new(true)));

        value.Should().BeOfType<PartitionValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(true), Index = 1});
    }

    [Test]
    public async Task Add_returnsExistingPartitionValue_keyExists()
    {
        await Storage.Add(new(true), new TestValue(true));

        var value = await Storage.Add(new(true), new(new(false)));

        value.Should().BeOfType<PartitionValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(false), Index = 2});
    }

    [Test]
    public async Task TryGet_returnsNone_noKey()
    {
        var value = await Storage.TryGet(new(true), index: 1);

        value.Should().Be((Option<TestValue>)Option.None);
    }

    [Test]
    public async Task TryGet_returnsExistingValue_keyExits()
    {
        await Storage.Add(new(true), new TestValue(true));

        var value = await Storage.TryGet(new(true), index: 1);

        value.Should().BeEquivalentTo(new {Value = new TestValue(true)});
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.Add(new(true), new TestValue(true));

        var value = await Storage.GetKeys().AsEnumerableAsync();

        value.Should().BeEquivalentTo(new[] {new TestKey(true)});
    }

    [Test]
    public async Task TryRemove_returnsZero_noKey()
    {
        var value = await Storage.TryRemove(new(true), upToIndex: 10);

        value.Should().Be(new None<TestValue>());
    }

    [Test]
    public async Task TryRemove_returnsOne_keyExists()
    {
        await Storage.Add(new(true), new TestValue(true));

        var value = await Storage.TryRemove(new(true), upToIndex: 10);

        value.Should().Be(new TestValue(true).AsOption());
    }

    [Test]
    public async Task TryRemove_returnsOne_twoKeysExist()
    {
        await Storage.Add(new(true), new TestValue(true));
        await Storage.Add(new(true), new TestValue(false));

        var value = await Storage.TryRemove(new(true), upToIndex: 1);
        value.Should().Be(new TestValue(true).AsOption());

        var value1 = await Storage.TryGet(new(true), index: 1);
        value1.Should().Be(new None<TestValue>());

        var value2 = await Storage.TryGet(new(true), index: 2);
        value2.Should().BeEquivalentTo(new TestValue(false).AsOption());

        var value3 = await Storage.TryRemove(new(true), upToIndex: 2);
        value3.Should().Be(new TestValue(false).AsOption());
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
    public void OneTimeSetup() =>
        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .Add<TestKey, TestValue>())
            .BuildServiceProvider();

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
    private ServiceProvider? Provider { get; set; }

    private IPartitionedAdminStorage<TestKey, TestValue> Storage => Provider!.GetRequiredService<IPartitionedAdminStorage<TestKey, TestValue>>();
}
