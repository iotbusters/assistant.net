using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Options;
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
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal;

public class MongoPartitionedStorageProviderIntegrationTests
{
    [Test]
    public async Task Add_returnsAddedValue_noKey()
    {
        var value1 = TestValue(version: 1);
        var value = await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(value1),
            updateFactory: (_, _) => throw new NotImplementedException());

        value.Should().Be(value1);
    }

    [Test]
    public async Task Add_returnsExistingValue_keyExists()
    {
        var value1 = TestValue(version: 1);
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

        value.Should().Be(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGet_returnsExistingValue_keyExits()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue(version: 1)),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.TryGet(TestKey, index: 1);

        value.Should().BeEquivalentTo(TestValue(version: 1).AsOption());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue(version: 1)),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.GetKeys(_ => true).ToArrayAsync();

        value.Should().BeEquivalentTo(new[] {TestKey});
    }

    [Test]
    public async Task TryRemove_returnsZero_noKey()
    {
        var option = await Storage.TryRemove(TestKey, upToIndex: 10);

        option.Should().Be(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsOne_keyExists()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue(version: 1)),
            updateFactory: (_, _) => throw new NotImplementedException());

        var option = await Storage.TryRemove(TestKey, upToIndex: 10);

        option.Should().BeEquivalentTo(TestValue(version: 1).AsOption());
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

        var option1 = await Storage.TryGet(TestKey, index: 1);
        option1.Should().BeEquivalentTo(TestValue(version: 1).AsOption());

        var option2 = await Storage.TryRemove(TestKey, upToIndex: 1);
        option2.Should().BeEquivalentTo(TestValue(version: 1).AsOption());

        var option3 = await Storage.TryGet(TestKey, index: 1);
        option3.Should().Be(new None<ValueRecord>());

        var option4 = await Storage.TryGet(TestKey, index: 2);
        option4.Should().BeEquivalentTo(TestValue(version: 2).AsOption());

        var option5 = await Storage.TryRemove(TestKey, upToIndex: 2);
        option5.Should().BeEquivalentTo(TestValue(version: 2).AsOption());
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseMongo(SetupMongo.ConfigureMongo)
                .AddMongoPartitioned<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestValue>>();
        var storage2 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.Add(key, new TestValue(true));
        var value = await storage2.TryGet(key, 1);

        value.Should().Be(Option.Some(new TestValue(true)));
    }

    [Test]
    public async Task TryGet_returnsNone_FromStorageOfAnotherValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseMongo(SetupMongo.ConfigureMongo)
                .AddMongoPartitioned<TestKey, TestBase>()
                .AddMongoPartitioned<TestKey, TestValue>())
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
                .UseMongo(SetupMongo.ConfigureMongo)
                .AddMongoPartitioned<TestKey, TestBase>()
                .AddMongoPartitioned<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IPartitionedStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.Add(key, new TestValue(true));
        await storage2.Add(key, new TestValue(false));
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
                .UseMongo(SetupMongo.ConfigureMongo)
                .AddMongoPartitioned<TestKey, TestValue>())
            .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
            .AddSystemClock(_ => TestDate)
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

    private static CancellationToken CancellationToken => new CancellationTokenSource(200).Token;

    private ValueRecord TestValue(int version = 1) => new(Content: Array.Empty<byte>(), Audit(version));
    private Audit Audit(int version) => new(version) {CorrelationId = TestCorrelationId, User = TestUser, Created = TestDate};
    private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", valueType: "test-value", content: Array.Empty<byte>());
    private string TestCorrelationId { get; } = Guid.NewGuid().ToString();
    private string TestUser { get; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;

    private ServiceProvider? Provider { get; set; }

    private IPartitionedStorageProvider<TestValue> Storage => (IPartitionedStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value.PartitionedProviders[typeof(TestValue)].Create(Provider!);
}
