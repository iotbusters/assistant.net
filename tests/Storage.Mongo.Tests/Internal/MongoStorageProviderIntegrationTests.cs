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
    public class MongoStorageProviderTestsIntegrationTests
    {
        [Test]
        public async Task AddOrGet_returnsAddedValue_notExists()
        {
            var value = await Storage.AddOrGet(TestKey, TestValue("added"));

            value.Should().BeEquivalentTo(new {Type = "added"});
        }

        [Test]
        public async Task AddOrGet_returnsExistingValue_exists()
        {
            await Storage.AddOrGet(TestKey, TestValue("added-1"));

            var value = await Storage.AddOrGet(TestKey, TestValue("added-2"));

            value.Should().BeEquivalentTo(
                TestValue("added-1") with {Audit = Audit()},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [TestCase(5)]
        public async Task AddOrGet_returnsValuesAndInitialVersion_concurrently(int concurrencyCount)
        {
            var tasks = Enumerable.Range(1, concurrencyCount).Select(
                i => Storage.AddOrGet(TestKey, TestValue($"value-{i}")));
            var values = await Task.WhenAll(tasks);

            var lastValue = await Storage.TryGet(TestKey);

            lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit()}});
            values.Select(x => x.Type).Distinct().Should().HaveCount(1);
        }

        [Test]
        public async Task AddOrUpdate_returnsAddedValue()
        {
            var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added"), (_, _) => TestValue("updated"));

            value.Should().BeEquivalentTo(
                TestValue("added") with {Audit = Audit()},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [Test]
        public async Task AddOrUpdate_returnsUpdatedValue()
        {
            await Storage.AddOrGet(TestKey, TestValue("added-1"));

            var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added-2"), (_, _) => TestValue("updated"));

            value.Should().BeEquivalentTo(
                TestValue("updated") with {Audit = Audit(2)},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [TestCase(5)]
        public async Task AddOrUpdate_returnsValuesAndLastVersion_concurrently(int concurrencyCount)
        {
            var tasks = Enumerable.Range(1, concurrencyCount).Select(
                i => Storage.AddOrUpdate(TestKey, TestValue($"value-{i}")));
            var values = await Task.WhenAll(tasks);

            var lastValue = await Storage.TryGet(TestKey);

            lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit(concurrencyCount)}});
            values.Should().BeEquivalentTo(Enumerable.Range(1, concurrencyCount).Select(i => new {Type = $"value-{i}"}));
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
            await Storage.AddOrGet(TestKey, TestValue("value"));

            var value = await Storage.TryGet(TestKey);

            value.Should().BeEquivalentTo(
                new {Value = TestValue("value") with {Audit = Audit()}},
                o => o.ComparingByMembers<ValueRecord>());
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
            await Storage.AddOrGet(TestKey, TestValue("value"));

            var value = await Storage.TryRemove(TestKey);

            value.Should().BeEquivalentTo(
                new {Value = TestValue("value") with {Audit = Audit()}},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [Test]
        public async Task GetKeys_returnsKeys()
        {
            await Storage.AddOrGet(TestKey, TestValue("value"));

            var value = Storage.GetKeys().ToArray();

            value.Should().BeEquivalentTo(new[] {TestKey});
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            var connectionString = "mongodb://127.0.0.1:27017";
            Provider = new ServiceCollection()
                .AddStorage(b => b
                    .UseMongo(o => o.ConnectionString = connectionString)
                    .AddMongo<TestKey, TestValue>())
                .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
                .AddSystemClock(_ => TestDate)
                .BuildServiceProvider();

            string pingContent;
            var mongoClient = Provider.GetRequiredService<IMongoClientFactory>().CreateClient();
            try
            {
                var ping = await mongoClient.GetDatabase("db").RunCommandAsync(
                    (Command<BsonDocument>)"{ping:1}",
                    ReadPreference.Nearest,
                    new CancellationTokenSource(1000).Token);
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
        public Task Cleanup() => MongoClient.DropDatabaseAsync(MongoNames.DatabaseName);

        private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", content: new byte[0]);
        private ValueRecord TestValue(string type) => new(Type: type, Content: new byte[0], new Audit(TestCorrelationId, TestUser));
        private Audit Audit(int version = 1) => new(new Audit(TestCorrelationId, TestUser).Details, version) {Created = TestDate};
        private string TestCorrelationId { get; set; } = Guid.NewGuid().ToString();
        private string TestUser { get; set; } = Guid.NewGuid().ToString();
        private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
        private ServiceProvider? Provider { get; set; }
        private IMongoClient MongoClient => Provider!.CreateScope().ServiceProvider.GetRequiredService<IMongoClientFactory>().CreateClient();
        private IStorageProvider<TestValue> Storage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IStorageProvider<TestValue>>();
    }
}
