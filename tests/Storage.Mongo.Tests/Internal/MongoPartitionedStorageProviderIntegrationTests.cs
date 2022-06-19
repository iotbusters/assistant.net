using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal;

public class MongoPartitionedStorageProviderIntegrationTests
{
    [Test]
    public async Task Add_returnsAddedValue_noKey()
    {
        var value = await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue("added")),
            updateFactory: (_, _) => throw new NotImplementedException());

        value.Should().Be(1);
    }

    [Test]
    public async Task Add_returnsExistingValue_keyExists()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue("added")),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.Add(
            TestKey,
            addFactory: _ => throw new NotImplementedException(),
            updateFactory: (_, _) => Task.FromResult(TestValue("added-2", version: 20)));

        value.Should().Be(20);
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
            addFactory: _ => Task.FromResult(TestValue("added")),
            updateFactory: (_, _) => throw new NotImplementedException());

        var value = await Storage.TryGet(TestKey, index: 1);

        value.Should().BeEquivalentTo(new {Value = TestValue("added")}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue("added")),
            updateFactory: (_, _) => throw new NotImplementedException());

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
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue("added")),
            updateFactory: (_, _) => throw new NotImplementedException());

        var count = await Storage.TryRemove(TestKey, upToIndex: 10);

        count.Should().Be(1);
    }

    [Test]
    public async Task TryRemove_returnsOne_twoKeysExist()
    {
        await Storage.Add(
            TestKey,
            addFactory: _ => Task.FromResult(TestValue("value-1", version: 1)),
            updateFactory: (_, _) => throw new NotImplementedException());
        await Storage.Add(
            TestKey,
            addFactory: _ => throw new NotImplementedException(),
            updateFactory: (_, _) => Task.FromResult(TestValue("value-2", version: 2)));

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
        const string connectionString = "mongodb://127.0.0.1:27017";
        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseMongo(o => o.ConnectionString = connectionString)
                .AddMongoPartitioned<TestKey, TestValue>())
            .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
            .AddSystemClock(_ => TestDate)
            .BuildServiceProvider();

        string pingContent;
        try
        {
            var ping = await MongoClient.GetDatabase("db").RunCommandAsync(
                (Command<BsonDocument>)"{ping:1}",
                ReadPreference.Nearest,
                CancellationToken);
            pingContent = ping.ToString();
        }
        catch
        {
            pingContent = string.Empty;
        }
        if (!pingContent.Contains("ok"))
            Assert.Ignore($"The tests require mongodb instance at {connectionString}.");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider?.Dispose();

    [SetUp, TearDown]
    public async Task Cleanup() => await MongoClient.DropDatabaseAsync(MongoNames.DatabaseName, CancellationToken);

    private static CancellationToken CancellationToken => new CancellationTokenSource(200).Token;
    private ValueRecord TestValue(string type, int version = 1) => new(Type: type, Content: Array.Empty<byte>(), Audit(version));
    private Audit Audit(int version) => new(TestCorrelationId, TestUser, TestDate, version);
    private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", content: Array.Empty<byte>(), valueType: "test-value");
    private string TestCorrelationId { get; } = Guid.NewGuid().ToString();
    private string TestUser { get; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
    private ServiceProvider? Provider { get; set; }
    private IMongoClient MongoClient => Provider!.CreateScope().ServiceProvider.GetRequiredService<IMongoClient>();

    private IPartitionedStorageProvider<TestValue> Storage => (IPartitionedStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value.PartitionedProviders[typeof(TestValue)].Create(Provider!);
}
