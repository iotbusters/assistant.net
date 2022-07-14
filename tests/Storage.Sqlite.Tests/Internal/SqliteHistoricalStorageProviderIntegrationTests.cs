using Assistant.Net.Abstractions;
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Internal;

public class SqliteHistoricalStorageProviderIntegrationTests
{
    [Test]
    public async Task AddOrGet_returnsAddedValue_notExists()
    {
        var value = await Storage.AddOrGet(TestKey, TestValue(version: 2));

        value.Should().BeEquivalentTo(TestValue(version: 2));
    }

    [Test]
    public async Task AddOrGet_returnsExistingValue_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.AddOrGet(TestKey, TestValue());

        value.Should().BeEquivalentTo(TestValue(), o => o.ComparingByMembers<ValueRecord>());
    }

    [TestCase(5)]
    public async Task AddOrGet_returnsValuesAndInitialVersion_concurrently(int concurrencyCount)
    {
        var tasks = Enumerable.Range(1, concurrencyCount).Select(_ =>
            Storage.AddOrGet(TestKey, TestValue()));
        var values = await Task.WhenAll(tasks);

        var lastValue = await Storage.TryGet(TestKey);

        lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit()}});
        values.Select(x => x.Audit.Version).Distinct().Should().HaveCount(1);
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedValue()
    {
        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue(), (_, _) => TestValue());

        value.Should().BeEquivalentTo(TestValue(), o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedValue()
    {
        await Storage.AddOrGet(TestKey, TestValue(version: 1));

        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue(version: 2), (_, _) => TestValue(version: 3));

        value.Should().BeEquivalentTo(TestValue(version: 3), o => o.ComparingByMembers<ValueRecord>());
    }

    [TestCase(5)]
    public async Task AddOrUpdate_returnsValuesAndOneOfRequestedVersions_concurrently(int concurrencyCount)
    {
        var requestedValues = Enumerable.Range(1, concurrencyCount).Select(i => TestValue(version: i)).ToArray();
        var tasks = requestedValues.Select(x => Storage.AddOrUpdate(TestKey, x));

        var values = await Task.WhenAll(tasks);
        values.Should().BeEquivalentTo(requestedValues);

        var lastValue = await Storage.TryGet(TestKey);
        lastValue.Should().BeOfType<Some<ValueRecord>>();
        requestedValues.Should().ContainEquivalentOf(lastValue.GetValueOrFail());
    }

    [TestCase(1000), Ignore("Manual run only")]
    public async Task AddOrUpdate_returnsUpdatedValueInTime_keysAndVersions(int count)
    {
        // arrange: storage population
        foreach (var i in Enumerable.Range(1, count))
        {
            var key = new KeyRecord(i.ToString(), "type", nameof(Mocks.TestValue), Array.Empty<byte>());
            await Storage.AddOrUpdate(key, TestValue(version: i));
            await Storage.AddOrUpdate(TestKey, TestValue(version: i));
        }

        // act: time measurement
        var watch = Stopwatch.StartNew();
        // act: operation
        var value = await Storage.AddOrUpdate(TestKey, TestValue(version: count + 1));
        watch.Stop();

        // assert
        watch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(0.1));
        value.Should().BeEquivalentTo(TestValue(version: count + 1), o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryGet_returnsNone_notExists()
    {
        var value = await Storage.TryGet(TestKey);

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGet_returnsSome_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.TryGet(TestKey);

        value.Should().BeEquivalentTo(new {Value = TestValue()}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryGetByVersion_returnsNone_notExists()
    {
        var value = await Storage.TryGet(TestKey, version: 1);

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGetByVersion_returnsSome_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.TryGet(TestKey, version: 1);

        value.Should().BeEquivalentTo(new { Value = TestValue() }, o => o.ComparingByMembers<ValueRecord>());
    }

    [TestCase(1000), Ignore("Manual run only")]
    public async Task TryGet_returnsValueInTime_keysAndVersions(int count)
    {
        // arrange: storage population
        foreach (var i in Enumerable.Range(1, count))
        {
            var key = new KeyRecord(i.ToString(), "type", nameof(Mocks.TestValue), Array.Empty<byte>());
            await Storage.AddOrUpdate(key, TestValue(version: i));
            await Storage.AddOrUpdate(TestKey, TestValue(version: i));
        }

        // act: time measurement
        var watch = Stopwatch.StartNew();
        // act: operation
        var value = await Storage.TryGet(TestKey);
        watch.Stop();

        // assert
        watch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(0.1));
        value.Should().BeEquivalentTo(
            TestValue(version: count), o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsNone_notExists()
    {
        var value = await Storage.TryRemove(TestKey);

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsSome_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.TryRemove(TestKey);

        value.Should().BeEquivalentTo(new {Value = TestValue()}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_doesNotLostVersions_AddOrUpdateConcurrently()
    {
        var requestedValues = Enumerable.Range(1, 5).Select(i => TestValue(version: i)).ToArray();
        await Storage.AddOrGet(TestKey, requestedValues[0]);

        var removeTask = Storage.TryRemove(TestKey);
        await Task.WhenAll(
            removeTask,
            Storage.AddOrUpdate(TestKey, requestedValues[1]),
            Storage.AddOrUpdate(TestKey, requestedValues[2]),
            Storage.AddOrUpdate(TestKey, requestedValues[3]),
            Storage.AddOrUpdate(TestKey, requestedValues[4]));

        var value = await Storage.TryGet(TestKey);
        var version1 = removeTask.Result.GetValueOrDefault()?.Audit.Version ?? 0;
        var version2 = value.GetValueOrDefault()?.Audit.Version ?? 0;
        version1.Should().BeLessOrEqualTo(4);
        version2.Should().Be(5);
    }

    [Test]
    public async Task TryRemoveByVersion_returnsNone_notExists()
    {
        var value = await Storage.TryRemove(TestKey, upToVersion: 1);

        value.Should().Be(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemoveByVersion_returnsSome_exists()
    {
        foreach (var i in Enumerable.Range(1, 5))
            await Storage.AddOrUpdate(TestKey, TestValue(version: i));

        var value = await Storage.TryRemove(TestKey, upToVersion: 4);

        value.Should().BeEquivalentTo(TestValue(version: 4).AsOption());
        var value5 = await Storage.TryGet(TestKey, version: 5);
        value5.Should().BeEquivalentTo(TestValue(version: 5).AsOption());
        var value4 = await Storage.TryGet(TestKey, version: 4);
        value4.Should().BeEquivalentTo(new None<ValueRecord>());
        var value1 = await Storage.TryGet(TestKey, version: 1);
        value1.Should().BeEquivalentTo(new None<ValueRecord>()); 
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.GetKeys(_ => true).ToArrayAsync();

        value.Should().BeEquivalentTo(new[] {TestKey});
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .AddSqliteHistorical<TestKey, TestValue>())
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
                .AddSqliteHistorical<TestKey, TestBase>()
                .AddSqliteHistorical<TestKey, TestValue>())
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
                .AddSqliteHistorical<TestKey, TestBase>()
                .AddSqliteHistorical<TestKey, TestValue>())
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
    public void OneTimeSetup()
    {
        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .AddSqliteHistorical<TestKey, TestValue>())
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

    private IHistoricalStorageProvider<TestValue> Storage => (IHistoricalStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value.HistoricalProviders[typeof(TestValue)].Create(Provider!);
}
