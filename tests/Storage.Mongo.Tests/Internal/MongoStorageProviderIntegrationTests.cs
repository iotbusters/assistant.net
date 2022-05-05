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

            value.Should().BeEquivalentTo(TestValue("added-1"), o => o.ComparingByMembers<ValueRecord>());
        }

        [TestCase(5)]
        public async Task AddOrGet_returnsValuesAndInitialVersion_concurrently(int concurrencyCount)
        {
            var tasks = Enumerable.Range(1, concurrencyCount).Select(
                i => Storage.AddOrGet(TestKey, TestValue($"value-{i}", version: 1)));
            var values = await Task.WhenAll(tasks);
            values.Select(x => x.Type).Distinct().Should().HaveCount(1);

            var lastValue = await Storage.TryGet(TestKey);
            lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit(version: 1)}});
        }

        [Test]
        public async Task AddOrUpdate_returnsAddedValue()
        {
            var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added"), (_, _) => TestValue("updated"));

            value.Should().BeEquivalentTo(TestValue("added"), o => o.ComparingByMembers<ValueRecord>());
        }

        [Test]
        public async Task AddOrUpdate_returnsUpdatedValue()
        {
            await Storage.AddOrGet(TestKey, TestValue("added-1"));

            var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added-2"), (_, _) => TestValue("updated"));

            value.Should().BeEquivalentTo(TestValue("updated"), o => o.ComparingByMembers<ValueRecord>());
        }

        [TestCase(5)]
        public async Task AddOrUpdate_returnsValuesAndOneOfRequestedVersions_concurrently(int concurrencyCount)
        {
            var requestedValues = Enumerable.Range(1, concurrencyCount).Select(i => TestValue($"value-{i}", version: i)).ToArray();
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
            await Storage.AddOrGet(TestKey, TestValue("value"));

            var value = await Storage.TryGet(TestKey);

            value.Should().BeEquivalentTo(new {Value = TestValue("value")}, o => o.ComparingByMembers<ValueRecord>());
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

            value.Should().BeEquivalentTo(new {Value = TestValue("value")}, o => o.ComparingByMembers<ValueRecord>());
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
        public Task Cleanup() => MongoClient.DropDatabaseAsync(MongoNames.DatabaseName, CancellationToken);

        private static CancellationToken CancellationToken => new CancellationTokenSource(200).Token;
        private ValueRecord TestValue(string type, int version = 1) => new(Type: type, Content: Array.Empty<byte>(), Audit(version));
        private Audit Audit(int version) => new(TestCorrelationId, TestUser, TestDate, version);
        private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", content: Array.Empty<byte>());
        private string TestCorrelationId { get; set; } = Guid.NewGuid().ToString();
        private string TestUser { get; set; } = Guid.NewGuid().ToString();
        private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
        private ServiceProvider? Provider { get; set; }
        private IMongoClient MongoClient => Provider!.CreateScope().ServiceProvider.GetRequiredService<IMongoClientFactory>().CreateClient();
        private IStorageProvider<TestValue> Storage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IStorageProvider<TestValue>>();
    }
}
