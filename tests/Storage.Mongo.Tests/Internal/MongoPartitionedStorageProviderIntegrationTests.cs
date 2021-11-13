using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
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

namespace Assistant.Net.Storage.Mongo.Tests.Internal
{
    public class MongoPartitionedStorageProviderIntegrationTests
    {
        [Test]
        public async Task Add_returnsAddedValue_noKey()
        {
            var value = await PartitionedStorage.Add(TestKey, TestValue("added"));

            value.Should().Be(1);
        }

        [Test]
        public async Task Add_returnsExistingValue_keyExists()
        {
            await PartitionedStorage.Add(TestKey, TestValue("added-1"));

            var value = await PartitionedStorage.Add(TestKey, TestValue("added-2"));

            value.Should().Be(2);
        }

        [Test]
        public async Task TryGet_throwsException_indexIsZero()
        {
            await PartitionedStorage.Awaiting(x => x.TryGet(TestKey, 0))
                .Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Test]
        public async Task TryGet_returnsNone_noKey()
        {
            var value = await PartitionedStorage.TryGet(TestKey, 1);

            value.Should().Be((Option<ValueRecord>)Option.None);
        }

        [Test]
        public async Task TryGet_returnsExistingValue_keyExits()
        {
            await PartitionedStorage.Add(TestKey, TestValue("added"));

            var value = await PartitionedStorage.TryGet(TestKey, 1);

            value.Should().BeEquivalentTo(
                new {Value = TestValue("added") with {Audit = Audit()}},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [Test]
        public async Task GetKeys_returnsKeys()
        {
            await MongoClient.DropDatabaseAsync(MongoNames.DatabaseName);
            await PartitionedStorage.Add(TestKey, TestValue("value"));

            var value = PartitionedStorage.GetKeys().ToArray();

            value.Should().BeEquivalentTo(TestKey);
        }

        [Test]
        public async Task TryRemove_returnsZero_noKey()
        {
            var count = await PartitionedStorage.TryRemove(TestKey, 10);

            count.Should().Be(0);
        }

        [Test]
        public async Task TryRemove_returnsOne_keyExists()
        {
            await PartitionedStorage.Add(TestKey, TestValue("value"));

            var count = await PartitionedStorage.TryRemove(TestKey, 10);

            count.Should().Be(1);
        }

        [Test]
        public async Task TryRemove_returnsOne_twoKeysExist()
        {
            await PartitionedStorage.Add(TestKey, TestValue("value-1"));
            await PartitionedStorage.Add(TestKey, TestValue("value-2"));

            var count1 = await PartitionedStorage.TryRemove(TestKey, 1);
            count1.Should().Be(1);

            var value1 = await PartitionedStorage.TryGet(TestKey, 1);
            value1.Should().Be((Option<ValueRecord>)Option.None);

            var value2 = await PartitionedStorage.TryGet(TestKey, 2);
            value2.Should().BeEquivalentTo(new {Value = new {Type = "value-2"}});

            var count2 = await PartitionedStorage.TryRemove(TestKey, 2);
            count2.Should().Be(1);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            const string connectionString = "mongodb://127.0.0.1:27017";
            Provider = new ServiceCollection()
                .AddStorage(b => b.AddMongoPartitioned<TestKey, TestValue>().UseMongo(o => o.ConnectionString = connectionString))
                .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
                .AddSystemClock(_ => TestDate)
                .BuildServiceProvider();

            string pingContent;
            try
            {
                var ping = await MongoClient.GetDatabase("db").RunCommandAsync(
                    (Command<BsonDocument>)"{ping:1}",
                    ReadPreference.Nearest,
                    new CancellationTokenSource(200).Token);
                pingContent = ping.ToString();
            }
            catch
            {
                pingContent = string.Empty;
            }
            if (!pingContent.Contains("ok"))
                Assert.Ignore($"The tests require mongodb instance at {connectionString}.");

            await MongoClient.DropDatabaseAsync(MongoNames.DatabaseName);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            try
            {
                await MongoClient.DropDatabaseAsync(MongoNames.DatabaseName, new CancellationTokenSource(200).Token);
            }
            catch { }

            await Provider.DisposeAsync();
        }

        [SetUp]
        public void Setup()
        {
            TestKey = new(id: $"test-{Guid.NewGuid()}", type: "test-key", content: new byte[0]);
            TestCorrelationId = Guid.NewGuid().ToString();
            TestUser = Guid.NewGuid().ToString();
            TestDate = DateTimeOffset.UtcNow;
        }

        private ValueRecord TestValue(string type) => new(Type: type, Content: new byte[0], new Audit(TestCorrelationId, TestUser));
        private Audit Audit(int version = 1) => new(new Audit(TestCorrelationId, TestUser).Details, version) { Created = TestDate };
        private KeyRecord TestKey { get; set; } = default!;
        private string TestCorrelationId { get; set; } = default!;
        private string TestUser { get; set; } = default!;
        private DateTimeOffset TestDate { get; set; }
        private ServiceProvider Provider { get; set; } = default!;
        private IMongoClient MongoClient => Provider!.CreateScope().ServiceProvider.GetRequiredService<IMongoClientFactory>().CreateClient();
        private IPartitionedStorageProvider<TestValue> PartitionedStorage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IPartitionedStorageProvider<TestValue>>();
    }
}
