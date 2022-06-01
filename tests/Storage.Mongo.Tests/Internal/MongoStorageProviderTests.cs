using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal;

public class MongoStorageProviderTests
{
    [Test]
    public async Task AddOrGet_callsFindAsync()
    {
        MongoCollectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<MongoRecord>>(),
            It.IsAny<FindOptions<MongoRecord>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new TestMongoCursor<MongoRecord>(new MongoRecord()));

        await Storage.AddOrGet(TestKey, TestValue);

        MongoCollectionMock.Verify(x => x.FindAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<FindOptions<MongoRecord>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        MongoCollectionMock.Verify(x => x.InsertOneAsync(
                It.IsAny<IClientSessionHandle>(),
                It.IsAny<MongoRecord>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AddOrGet_callsFindAsyncAndInsertOneAsync()
    {
        await Storage.AddOrGet(TestKey, TestValue);

        MongoCollectionMock.Verify(x => x.FindAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<FindOptions<MongoRecord>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        MongoCollectionMock.Verify(x => x.InsertOneAsync(
                It.IsAny<MongoRecord>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task AddOrUpdate_callsFindAsyncAndInsertOneAsync()
    {
        await Storage.AddOrUpdate(TestKey, TestValue);

        MongoCollectionMock.Verify(x => x.FindAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<FindOptions<MongoRecord>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        MongoCollectionMock.Verify(x => x.InsertOneAsync(
                It.IsAny<MongoRecord>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        MongoCollectionMock.Verify(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<MongoRecord>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AddOrUpdate_callsFindAsyncAndReplaceOneAsync()
    {
        var record = new MongoRecord {Details = Audit.Details};
        MongoCollectionMock.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<MongoRecord>>(),
            It.IsAny<FindOptions<MongoRecord>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new TestMongoCursor<MongoRecord>(record));
        MongoCollectionMock.Setup(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<MongoRecord>>(),
            It.IsAny<MongoRecord>(),
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 0, new BsonString("")));

        await Storage.AddOrUpdate(TestKey, TestValue);

        MongoCollectionMock.Verify(x => x.FindAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<FindOptions<MongoRecord>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        MongoCollectionMock.Verify(x => x.InsertOneAsync(
                It.IsAny<MongoRecord>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        MongoCollectionMock.Verify(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<MongoRecord>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task TryGet_callsFindAsync()
    {
        await Storage.TryGet(TestKey);

        MongoCollectionMock.Verify(x => x.FindAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<FindOptions<MongoRecord>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task TryRemove_callsFindOneAndDeleteAsync()
    {
        await Storage.TryRemove(TestKey);

        MongoCollectionMock.Verify(x => x.FindOneAndDeleteAsync(
                It.IsAny<FilterDefinition<MongoRecord>>(),
                It.IsAny<FindOneAndDeleteOptions<MongoRecord>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void GetKeys_callsAggregate()
    {
        var _ = Storage.GetKeys().ToArray();

        MongoCollectionMock.Verify(x => x.Aggregate(
                It.IsAny<PipelineDefinition<MongoRecord, KeyRecord>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [SetUp]
    public void Setup()
    {
        var mongoClientMock = new Mock<IMongoClient> {DefaultValue = DefaultValue.Mock};
        var mongoDatabaseMock = new Mock<IMongoDatabase> {DefaultValue = DefaultValue.Mock};

        MongoCollectionMock = MockCollection<MongoRecord>(mongoDatabaseMock);

        TestKey = new KeyRecord(id: Guid.NewGuid().ToString(), type: "key", content: Array.Empty<byte>());

        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseMongo(o => o.ConnectionString = "mongodb://127.0.0.1:27017")
                .AddMongo<TestKey, TestValue>())
            .ReplaceSingleton(_ => mongoClientMock.Object)
            .ReplaceSingleton(_ => mongoDatabaseMock.Object)
            .BuildServiceProvider();
    }

    [TearDown]
    public void TearDown() => Provider?.Dispose();

    private Mock<IMongoCollection<T>> MockCollection<T>(Mock<IMongoDatabase> mongoDatabaseMock)
    {
        var mock = new Mock<IMongoCollection<T>> {DefaultValue = DefaultValue.Mock};

        mock.Setup(x => x.CollectionNamespace).Returns(new CollectionNamespace("db", typeof(T).Name));
        mock.Setup(x => x.DocumentSerializer).Returns(new BsonClassMapSerializer<T>(BsonClassMap.LookupClassMap(typeof(T))));
        mongoDatabaseMock.Setup(x => x.GetCollection<T>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>())).Returns(mock.Object);

        return mock;
    }

    private Mock<IMongoCollection<MongoRecord>> MongoCollectionMock { get; set; } = default!;
    private KeyRecord TestKey { get; set; } = default!;
    private ValueRecord TestValue => new(Type: "type", Content: Array.Empty<byte>(), Audit);
    private Audit Audit => new(TestCorrelationId, TestUser, TestDate, version: 1);
    private string TestCorrelationId { get; set; } = Guid.NewGuid().ToString();
    private string TestUser { get; set; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; set; } = DateTimeOffset.UtcNow;
    private ServiceProvider? Provider { get; set; }

    private IStorageProvider<TestValue> Storage => (IStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value.Providers[typeof(TestValue)].Create(Provider!);
}
