using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Internal;

public class SqliteHistoricalStorageIntegrationTests
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

        var value = await Storage.AddOrGet(new(true), new TestValue(true));

        value.Should().BeEquivalentTo(new TestValue(true));
    }

    [Test]
    public async Task AddOrGet_returnsAddedHistoricalValue_notExists()
    {
        var value = await Storage.AddOrGet(new(true), new StorageValue<TestValue>(new(true)));

        value.Should().BeOfType<HistoricalValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(true), Version = 1});
    }

    [Test]
    public async Task AddOrGet_returnsExistingHistoricalValue_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.AddOrGet(new(true), new StorageValue<TestValue>(new(false)));

        value.Should().BeOfType<HistoricalValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(true), Version = 1});
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
    public async Task AddOrUpdate_returnsAddedHistoricalValue()
    {
        var value = await Storage.AddOrUpdate(
            new(true),
            _ => new(new(true)),
            (_, _) => new(new(false)));

        value.Should().BeOfType<HistoricalValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(true), Version = 1});
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedHistoricalValue()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.AddOrUpdate(
            new(true),
            _ => new(new(true)),
            (_, _) => new(new(false)));

        value.Should().BeOfType<HistoricalValue<TestValue>>()
            .And.BeEquivalentTo(new {Value = new TestValue(false), Version = 2});
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

        value.Should().BeEquivalentTo(new None<HistoricalValue<ValueRecord>>());
    }

    [Test]
    public async Task TryGetDetailed_returns_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.TryGetDetailed(new(true));

        value.Should().BeOfType<Some<HistoricalValue<TestValue>>>()
            .And.BeEquivalentTo(new {Value = new {Value = new TestValue(true), Version = 1}});
    }

    [Test]
    public async Task TryGetByVersion_returnsNone_notExists()
    {
        var value = await Storage.TryGet(new(true), version: 1);

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGetByVersion_returnsSome_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.TryGet(new(true), version: 1);

        value.Should().BeEquivalentTo(new {Value = new TestValue(true)}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryGetDetailedByVersion_returnsNone_notExists()
    {
        var value = await Storage.TryGetDetailed(new(true), version: 1);

        value.Should().BeEquivalentTo(new None<HistoricalValue<ValueRecord>>());
    }

    [Test]
    public async Task TryGetDetailedByVersion_returnsSome_exists()
    {
        await Storage.AddOrGet(new(true), new TestValue(true));

        var value = await Storage.TryGetDetailed(new(true), version: 1);

        value.Should().BeOfType<Some<HistoricalValue<TestValue>>>()
            .And.BeEquivalentTo(new { Value = new { Value = new TestValue(true), Version = 1 } });
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
    public async Task TryRemoveByVersion_returnsNone_notExists()
    {
        var value = await Storage.TryRemove(new(true), upToVersion: 1);

        value.Should().Be(new None<TestValue>());
    }

    [Test]
    public async Task TryRemoveByVersion_returnsSome_exists()
    {
        var tasks = Enumerable.Range(1, 5).Select(_ => Storage.AddOrUpdate(new(true), new TestValue(true)));
        await Task.WhenAll(tasks);

        var value = await Storage.TryRemove(new(true), upToVersion: 4);

        value.Should().Be(new TestValue(true).AsOption());
        var value5 = await Storage.TryGet(new(true), version: 5);
        value5.Should().BeEquivalentTo(new TestValue(true).AsOption());
        var value4 = await Storage.TryGet(new(true), version: 4);
        value4.Should().BeEquivalentTo(new None<ValueRecord>());
        var value1 = await Storage.TryGet(new(true), version: 1);
        value1.Should().BeEquivalentTo(new None<ValueRecord>()); 
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

        var storage1 = provider.GetRequiredService<IHistoricalStorage<TestKey, TestValue>>();
        var storage2 = provider.GetRequiredService<IHistoricalStorage<TestKey, TestValue>>();

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

        var storage1 = provider.GetRequiredService<IHistoricalStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IHistoricalStorage<TestKey, TestValue>>();

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

        var storage1 = provider.GetRequiredService<IHistoricalStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IHistoricalStorage<TestKey, TestValue>>();

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
        dbContext.HistoricalKeys.RemoveRange(dbContext.HistoricalKeys);
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private static CancellationToken CancellationToken => new CancellationTokenSource(100).Token;
    private ServiceProvider? Provider { get; set; }

    private IHistoricalAdminStorage<TestKey, TestValue> Storage => Provider!.GetRequiredService<IHistoricalAdminStorage<TestKey, TestValue>>();
}
