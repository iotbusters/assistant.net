using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal;

public class MongoHistoricalStorageProviderIntegrationTests
{
    [Test]
    public async Task AddOrGet_returnsAddedValue_notExists()
    {
        var value = await Storage.AddOrGet(TestKey, TestValue(version: 1));

        value.Should().BeEquivalentTo(TestValue(version: 1));
    }

    [Test]
    public async Task AddOrGet_returnsExistingValue_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue(version: 1));

        var value = await Storage.AddOrGet(TestKey, TestValue(version: 2));

        value.Should().BeEquivalentTo(TestValue(version: 1), o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedValue()
    {
        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue(version: 1), (_, _) => TestValue(version: 2));

        value.Should().BeEquivalentTo(TestValue(version: 1), o => o.ComparingByMembers<ValueRecord>());
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
        var requestedValues = Enumerable.Range(1, concurrencyCount).Select(TestValue).ToArray();
        var tasks = requestedValues.Select(x => Storage.AddOrUpdate(TestKey, x));

        var values = await Task.WhenAll(tasks);
        values.Should().BeEquivalentTo(requestedValues);

        var lastValue = await Storage.TryGet(TestKey);
        lastValue.Should().BeOfType<Some<ValueRecord>>();
        requestedValues.Should().ContainEquivalentOf(lastValue.GetValueOrFail());
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
        await Storage.AddOrGet(TestKey, TestValue(version: 1));

        var value = await Storage.TryGet(TestKey);

        value.Should().BeEquivalentTo(new {Value = TestValue(version: 1)}, o => o.ComparingByMembers<ValueRecord>());
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
        await Storage.AddOrGet(TestKey, TestValue(version: 1));

        var value = await Storage.TryGet(TestKey, version: 1);

        value.Should().BeEquivalentTo(new {Value = TestValue(version: 1)}, o => o.ComparingByMembers<ValueRecord>());
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
        await Storage.AddOrGet(TestKey, TestValue(version: 1));

        var value = await Storage.TryRemove(TestKey);

        value.Should().BeEquivalentTo(new {Value = TestValue(version: 1)}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_doesNotLostVersions_AddOrUpdateConcurrently()
    {
        var requestedValues = Enumerable.Range(1, 5).Select(TestValue).ToArray();
        await Storage.AddOrGet(TestKey, requestedValues[0]);

        var removeTask = Storage.TryRemove(TestKey);
        await Task.WhenAll(
            removeTask,
            Storage.AddOrUpdate(TestKey, requestedValues[1]),
            Storage.AddOrUpdate(TestKey, requestedValues[2]),
            Storage.AddOrUpdate(TestKey, requestedValues[3]),
            Storage.AddOrUpdate(TestKey, requestedValues[4]));

        var value = await Storage.TryGet(TestKey);
        var removedVersion = removeTask.Result.GetValueOrDefault()?.Audit.Version ?? 0;
        var storedVersion = value.GetValueOrDefault()?.Audit.Version ?? 0;
        (removedVersion + storedVersion)
            .Should().Be(5, $"5 add/update operations should have number 5 ({removedVersion}+{storedVersion}) in total");
    }

    [Test]
    public async Task TryRemoveByVersion_returnsNone_notExists()
    {
        var option = await Storage.TryRemove(TestKey, upToVersion: 1);

        option.Should().BeEquivalentTo(new None<TestValue>());
    }

    [Test]
    public async Task TryRemoveByVersion_returnsSome_exists()
    {
        foreach (var i in Enumerable.Range(1, 5))
            await Storage.AddOrUpdate(TestKey, TestValue(version: i));

        var option = await Storage.TryRemove(TestKey, upToVersion: 4);

        option.Should().BeEquivalentTo(new {Value = TestValue(version: 4)});
        var value5 = await Storage.TryGet(TestKey, version: 5);
        value5.Should().BeEquivalentTo(
            new {Value = TestValue(version: 5)},
            o => o.ComparingByMembers<ValueRecord>());
        var value4 = await Storage.TryGet(TestKey, version: 4);
        value4.Should().BeEquivalentTo(new None<ValueRecord>());
        var value1 = await Storage.TryGet(TestKey, version: 1);
        value1.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.AddOrGet(TestKey, TestValue(version: 1));

        var value = await Storage.GetKeys(_ => true).ToArrayAsync();

        value.Should().BeEquivalentTo(new[] {TestKey});
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseMongo(SetupMongo.ConfigureMongo)
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
                .UseMongo(SetupMongo.ConfigureMongo)
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
                .UseMongo(SetupMongo.ConfigureMongo)
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
                .UseMongo(SetupMongo.ConfigureMongo)
                .Add<TestKey, TestValue>())
            .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
            .AddSystemClock(_ => TestDate)
            .BuildServiceProvider();

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider?.Dispose();

    [SetUp]
    public async Task Cleanup()
    {
        var database = Provider!.GetRequiredService<IOptions<MongoOptions>>().Value.DatabaseName;
        await Provider!.GetRequiredService<IMongoClient>().DropDatabaseAsync(database, CancellationToken);
    }

    private static CancellationToken CancellationToken => new CancellationTokenSource(200).Token;

    private ValueRecord TestValue(int version = 1) => new(Content: Array.Empty<byte>(), Audit(version));
    private Audit Audit(int version) => new(version) {CorrelationId = TestCorrelationId, User = TestUser, Created = TestDate};
    private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", valueType: "type", content: Array.Empty<byte>());
    private string TestCorrelationId { get; } = Guid.NewGuid().ToString();
    private string TestUser { get; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
    private ServiceProvider? Provider { get; set; }

    private IHistoricalStorageProvider<TestValue> Storage => (IHistoricalStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value.HistoricalStorageProviderFactory!.Create(Provider!, typeof(TestValue));
}
