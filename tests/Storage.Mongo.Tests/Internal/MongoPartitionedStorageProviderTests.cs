using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal
{
    public class MongoPartitionedStorageProviderTests
    {
        [Test]
        public async Task Add_callsAggregateAsyncAndInsertOneAsync()
        {
            await Storage.Add(TestKey, TestValue);

            KeyCollectionMock.Verify(x => x.InsertOneAsync(
                    It.IsAny<MongoPartitionKeyRecord>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            KeyValueCollectionMock.Verify(x => x.AggregateAsync(
                    It.IsAny<PipelineDefinition<MongoPartitionKeyValueRecord, long>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            KeyValueCollectionMock.Verify(x => x.InsertOneAsync(
                    It.IsAny<MongoPartitionKeyValueRecord>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            ValueCollectionMock.Verify(x => x.InsertOneAsync(
                    It.IsAny<MongoPartitionValueRecord>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Add_callsInsertOneAsync()
        {
            KeyValueCollectionMock.Setup(x => x.AggregateAsync(
                It.IsAny<PipelineDefinition<MongoPartitionKeyValueRecord, long>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new TestMongoCursor<long>(1));

            await Storage.Add(TestKey, TestValue);

            KeyCollectionMock.Verify(x => x.InsertOneAsync(
                    It.IsAny<MongoPartitionKeyRecord>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            KeyValueCollectionMock.Verify(x => x.AggregateAsync(
                    It.IsAny<PipelineDefinition<MongoPartitionKeyValueRecord, long>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            KeyValueCollectionMock.Verify(x => x.InsertOneAsync(
                    It.IsAny<MongoPartitionKeyValueRecord>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            ValueCollectionMock.Verify(x => x.InsertOneAsync(
                    It.IsAny<MongoPartitionValueRecord>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task TryGet_callsAggregateAsync()
        {
            await Storage.TryGet(TestKey, 1);

            KeyValueCollectionMock.Verify(x => x.AggregateAsync(
                    It.IsAny<PipelineDefinition<MongoPartitionKeyValueRecord, ValueRecord>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task TryRemove_callsAggregateAsyncAndDeleteManyAsync()
        {
            KeyCollectionMock.Setup(x => x.AggregateAsync(
                It.IsAny<PipelineDefinition<MongoPartitionKeyRecord, PartitionKey>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new TestMongoCursor<PartitionKey>(new PartitionKey("else", 2)));
            KeyValueCollectionMock.Setup(x => x.AggregateAsync(
                It.IsAny<PipelineDefinition<MongoPartitionKeyValueRecord, PartitionKey>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new TestMongoCursor<PartitionKey>(new PartitionKey("whatever", 1)));
            ValueCollectionMock.Setup(x => x.AggregateAsync(
                It.IsAny<PipelineDefinition<MongoPartitionValueRecord, Guid>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new TestMongoCursor<Guid>(Guid.NewGuid()));

            await Storage.TryRemove(TestKey, 1);

            KeyCollectionMock.Verify(x => x.AggregateAsync(
                    It.IsAny<PipelineDefinition<MongoPartitionKeyRecord, PartitionKey>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            KeyCollectionMock.Verify(x => x.DeleteManyAsync(
                    It.IsAny<FilterDefinition<MongoPartitionKeyRecord>>(),
                    It.IsAny<DeleteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            KeyValueCollectionMock.Verify(x => x.AggregateAsync(
                    It.IsAny<PipelineDefinition<MongoPartitionKeyValueRecord, PartitionKey>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            KeyValueCollectionMock.Verify(x => x.DeleteManyAsync(
                    It.IsAny<FilterDefinition<MongoPartitionKeyValueRecord>>(),
                    It.IsAny<DeleteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            ValueCollectionMock.Verify(x => x.AggregateAsync(
                    It.IsAny<PipelineDefinition<MongoPartitionValueRecord, Guid>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            ValueCollectionMock.Verify(x => x.DeleteManyAsync(
                    It.IsAny<FilterDefinition<MongoPartitionValueRecord>>(),
                    It.IsAny<DeleteOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void GetKeys_callsAggregate()
        {
            var _ = Storage.GetKeys().ToArray();

            KeyCollectionMock.Verify(x => x.Aggregate(
                    It.IsAny<PipelineDefinition<MongoPartitionKeyRecord, KeyRecord>>(),
                    It.IsAny<AggregateOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [SetUp]
        public void Setup()
        {
            var mongoClientMock = new Mock<IMongoClient> { DefaultValue = DefaultValue.Mock };
            var mongoDatabaseMock = new Mock<IMongoDatabase> { DefaultValue = DefaultValue.Mock };
            mongoClientMock.Setup(x => x.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>())).Returns(mongoDatabaseMock.Object);

            KeyCollectionMock = MockCollection<MongoPartitionKeyRecord>(mongoDatabaseMock);
            KeyValueCollectionMock = MockCollection<MongoPartitionKeyValueRecord>(mongoDatabaseMock);
            ValueCollectionMock = MockCollection<MongoPartitionValueRecord>(mongoDatabaseMock);

            TestKey = new KeyRecord(id: Guid.NewGuid().ToString(), type: "key", content: new byte[0]);

            Provider = new ServiceCollection()
                .ConfigureMongoOptions(o => o.ConnectionString = "mongodb://127.0.0.1:27017")
                .AddStorage(b => b.AddMongoPartitioned<TestKey, TestValue>())
                .ReplaceSingleton(_ => mongoClientMock.Object)
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

        private Mock<IMongoCollection<MongoPartitionKeyRecord>> KeyCollectionMock { get; set; } = default!;
        private Mock<IMongoCollection<MongoPartitionKeyValueRecord>> KeyValueCollectionMock { get; set; } = default!;
        private Mock<IMongoCollection<MongoPartitionValueRecord>> ValueCollectionMock { get; set; } = default!;
        private KeyRecord TestKey { get; set; } = default!;
        private ValueRecord TestValue => new(Type: "type", Content: new byte[0]);
        private ServiceProvider? Provider { get; set; }
        private IPartitionedStorageProvider<TestValue> Storage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IPartitionedStorageProvider<TestValue>>();
    }
}
