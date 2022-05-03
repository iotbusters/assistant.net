using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task Add_callsHistoricalProviderAddOrUpdate()
        {
            HistoricalStorageProviderMock.Setup(x => x.AddOrUpdate(
                    It.IsAny<KeyRecord>(),
                    It.IsAny<Func<KeyRecord, Task<ValueRecord>>>(),
                    It.IsAny<Func<KeyRecord, ValueRecord, Task<ValueRecord>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestValue);

            await Storage.Add(TestKey, TestValue);

            HistoricalStorageProviderMock.Verify(x => x.AddOrUpdate(
                    It.IsAny<KeyRecord>(),
                    It.IsAny<Func<KeyRecord, Task<ValueRecord>>>(),
                    It.IsAny<Func<KeyRecord, ValueRecord, Task<ValueRecord>>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task TryGet_callsHistoricalProviderTryGet()
        {
            await Storage.TryGet(TestKey, index: 1);

            HistoricalStorageProviderMock.Verify(x => x.TryGet(
                    It.IsAny<KeyRecord>(),
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task TryRemove_callsHistoricalProviderTryRemove()
        {
            await Storage.TryRemove(TestKey, upToIndex: 1);

            HistoricalStorageProviderMock.Verify(x => x.TryRemove(
                    It.IsAny<KeyRecord>(),
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void GetKeys_callsHistoricalProviderGetKeys()
        {
            var _ = Storage.GetKeys().ToArray();

            HistoricalStorageProviderMock.Verify(x => x.GetKeys(), Times.Once);
        }

        [SetUp]
        public void Setup()
        {
            HistoricalStorageProviderMock = new Mock<IHistoricalStorageProvider<TestValue>> { DefaultValue = DefaultValue.Mock };

            Provider = new ServiceCollection()
                .ConfigureMongoOptions(o => o.ConnectionString = "mongodb://127.0.0.1:27017")
                .AddStorage(b => b.AddMongoPartitioned<TestKey, TestValue>())
                .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
                .ReplaceSingleton(_ => HistoricalStorageProviderMock.Object)
                .BuildServiceProvider();
        }

        [TearDown]
        public void TearDown() => Provider.Dispose();
        
        private Mock<IHistoricalStorageProvider<TestValue>> HistoricalStorageProviderMock { get; set; } = default!;
        private KeyRecord TestKey { get; } = new(id: Guid.NewGuid().ToString(), type: "key", content: Array.Empty<byte>());
        private ValueRecord TestValue => new(Type: "type", Content: Array.Empty<byte>(), new Audit(TestCorrelationId, TestUser, created: DateTimeOffset.UtcNow, version: 1));
        private string TestCorrelationId { get; } = Guid.NewGuid().ToString();
        private string TestUser { get;  } = Guid.NewGuid().ToString();
        private ServiceProvider Provider { get; set; } = default!;
        private IPartitionedStorageProvider<TestValue> Storage => Provider.CreateScope().ServiceProvider.GetRequiredService<IPartitionedStorageProvider<TestValue>>();
    }
}
