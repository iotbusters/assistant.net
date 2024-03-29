using Assistant.Net.Storage.Abstractions;
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

public class SqliteStorageIntegrationTests
{
    [Test]
    public async Task AddOrGet_returnsAddedValue_notExists()
    {
        var value = await Storage.AddOrGet(new(true), new TestValue(true));

        value.Should().BeEquivalentTo(new TestValue(true));
    }

    [Test]
    public async Task AddOrGet_returnsExistingValue_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.AddOrGet(new(true), new TestValue(false));

        value.Should().BeEquivalentTo(new TestValue(true));
    }

    [Test]
    public async Task AddOrGet_returnsAddedStorageValue_notExists()
    {
        var value = await Storage.AddOrGet(new(true), new StorageValue<TestValue>(new(true)));

        value.Should().BeOfType<StorageValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(true)});
    }

    [Test]
    public async Task AddOrGet_returnsExistingStorageValue_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.AddOrGet(new(true), new StorageValue<TestValue>(new(false)));

        value.Should().BeOfType<StorageValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(true)});
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedValue()
    {
        var value = await Storage.AddOrUpdate(new(true), _ => new(true), (_, _) => new(false));

        value.Should().BeEquivalentTo(new TestValue(true));
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedValue()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.AddOrUpdate(new(true), _ => new(true), (_, _) => new(false));

        value.Should().BeEquivalentTo(new TestValue(false));
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedStorageValue()
    {
        var value = await Storage.AddOrUpdate(
            new(true),
            _ => new(new(true)),
            (_, _) => new(new(false)));

        value.Should().BeOfType<StorageValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(true)});
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedStorageValue()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.AddOrUpdate(
            new(true),
            _ => new(new(true)),
            (_, _) => new(new(false)));

        value.Should().BeOfType<StorageValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(false)});
    }

    [Test]
    public async Task TryGet_returnsNone_notExists()
    {
        var value = await Storage.TryGet(new(true));

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGet_returnsSome_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.TryGet(new(true));

        value.Should().BeEquivalentTo(new {Value = new TestValue(true)}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryGetDetailed_returnsNone_notExists()
    {
        var value = await Storage.TryGetDetailed(new(true));

        value.Should().BeEquivalentTo(new None<StorageValue<ValueRecord>>());
    }

    [Test]
    public async Task TryGetDetailed_returns_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.TryGetDetailed(new(true));

        value.Should().BeOfType<Some<StorageValue<TestValue>>>()
            .And.BeEquivalentTo(new {Value = new {Value = new TestValue(true)}});
    }

    [Test]
    public async Task TryRemove_returnsNone_notExists()
    {
        var value = await Storage.TryRemove(new(true));

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsSome_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.TryRemove(new(true));

        value.Should().BeEquivalentTo(new {Value = new TestValue(true)}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.GetKeys().AsEnumerableAsync();

        value.Should().BeEquivalentTo(new[] {new TestKey(true)});
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .Add<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();
        var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.AddOrGet(key, new TestValue(true));
        var value = await storage2.TryGet(key);

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

        var storage1 = provider.GetRequiredService<IStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.AddOrGet(key, new TestValue(true));
        var value = await storage2.TryGet(key);

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

        var storage1 = provider.GetRequiredService<IStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.AddOrGet(key, new TestValue(true));
        await storage2.AddOrGet(key, new TestValue(false));
        var value1 = await storage1.TryGet(key);
        var value2 = await storage2.TryGet(key);

        value1.Should().Be(Option.Some<TestBase>(new TestValue(true)));
        value2.Should().Be(Option.Some(new TestValue(false)));
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
        dbContext.StorageKeys.RemoveRange(dbContext.StorageKeys);
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private static CancellationToken CancellationToken => new CancellationTokenSource(100).Token;
    private ServiceProvider? Provider { get; set; }

    private IAdminStorage<TestKey, TestValue> Storage => Provider!.GetRequiredService<IAdminStorage<TestKey, TestValue>>();
}
