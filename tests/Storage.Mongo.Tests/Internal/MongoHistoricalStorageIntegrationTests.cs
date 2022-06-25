using Assistant.Net.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal;

public class MongoHistoricalStorageIntegrationTests
{
    [Test]
    public async Task AddOrGet_returnsAddedValue_notExists()
    {
        var value = await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        value.Should().BeEquivalentTo(new TestValue(true));
    }

    [Test]
    public async Task AddOrGet_returnsExistingValue_exists()
    {
        await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        var value = await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        value.Should().BeEquivalentTo(new TestValue(true));
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedValue()
    {
        var value = await Storage.AddOrUpdate(new TestKey(true), _ => new TestValue(true), (_, _) => new TestValue(false));

        value.Should().BeEquivalentTo(new TestValue(true));
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedValue()
    {
        await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        var value = await Storage.AddOrUpdate(new TestKey(true), _ => new TestValue(true), (_, _) => new TestValue(false));

        value.Should().BeEquivalentTo(new TestValue(false));
    }

    [Test]
    public async Task TryGet_returnsNone_notExists()
    {
        var value = await Storage.TryGet(new TestKey(true));

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGet_returnsSome_exists()
    {
        await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        var value = await Storage.TryGet(new TestKey(true));

        value.Should().BeEquivalentTo(new {Value = new TestValue(true)}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryGetByVersion_returnsNone_notExists()
    {
        var value = await Storage.TryGet(new TestKey(true), version: 1);

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGetByVersion_returnsSome_exists()
    {
        await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        var value = await Storage.TryGet(new TestKey(true), version: 1);

        value.Should().BeEquivalentTo(new { Value = new TestValue(true) }, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsNone_notExists()
    {
        var value = await Storage.TryRemove(new TestKey(true));

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsSome_exists()
    {
        await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        var value = await Storage.TryRemove(new TestKey(true));

        value.Should().BeEquivalentTo(new {Value = new TestValue(true)}, o => o.ComparingByMembers<ValueRecord>());
    }
    
    [Test]
    public async Task TryRemoveByVersion_returnsNone_notExists()
    {
        var count = await Storage.TryRemove(new TestKey(true), upToVersion: 1);

        count.Should().Be(0L);
    }

    [Test]
    public async Task TryRemoveByVersion_returnsSome_exists()
    {
        var tasks = Enumerable.Range(1, 5).Select(_ => Storage.AddOrUpdate(new TestKey(true), new TestValue(true)));
        await Task.WhenAll(tasks);

        var count = await Storage.TryRemove(new TestKey(true), upToVersion: 4);

        count.Should().Be(4);
        var value5 = await Storage.TryGet(new TestKey(true), version: 5);
        value5.Should().BeEquivalentTo(new {Value = new TestValue(true)});
        var value4 = await Storage.TryGet(new TestKey(true), version: 4);
        value4.Should().BeEquivalentTo(new None<ValueRecord>());
        var value1 = await Storage.TryGet(new TestKey(true), version: 1);
        value1.Should().BeEquivalentTo(new None<ValueRecord>()); 
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.AddOrGet(new TestKey(true), new TestValue(true));

        var value = await Storage.GetKeys().AsEnumerableAsync();

        value.Should().BeEquivalentTo(new[] {new TestKey(true)});
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseMongo(SetupMongo.ConfigureMongo)
                .AddMongoHistorical<TestKey, TestValue>())
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
                .AddMongoHistorical<TestKey, TestBase>()
                .AddMongoHistorical<TestKey, TestValue>())
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
                .AddMongoHistorical<TestKey, TestBase>()
                .AddMongoHistorical<TestKey, TestValue>())
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
                .AddMongoHistorical<TestKey, TestValue>())
            .BuildServiceProvider();

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider?.Dispose();

    [SetUp]
    public async Task Cleanup()
    {
        var provider = Provider!.CreateScope().ServiceProvider;
        var database = provider.GetRequiredService<IOptions<MongoOptions>>().Value.DatabaseName;
        await provider.GetRequiredService<IMongoClient>().DropDatabaseAsync(database, CancellationToken);
    }

    private static CancellationToken CancellationToken => new CancellationTokenSource(100).Token;

    private ServiceProvider? Provider { get; set; }

    private IHistoricalAdminStorage<TestKey, TestValue> Storage => Provider!.GetRequiredService<IHistoricalAdminStorage<TestKey, TestValue>>();
}
