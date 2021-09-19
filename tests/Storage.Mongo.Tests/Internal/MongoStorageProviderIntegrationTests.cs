using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
        public async Task AddOrGet_returnsAddedValue_notExits()
        {
            var value = await Storage.AddOrGet(TestKey, TestValue("added"));

            value.Should().BeEquivalentTo(new {Type = "added"});
        }

        [Test]
        public async Task AddOrGet_returnsExistingValue_exits()
        {
            await Storage.AddOrGet(TestKey, TestValue("added-1"));

            var value = await Storage.AddOrGet(TestKey, TestValue("added-2"));

            value.Should().BeEquivalentTo(
                TestValue("added-1") with {Audit = Audit.Initial(TestDate)},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [TestCase(5)]
        public async Task AddOrGet_returnsValuesAndInitialVersion_concurrently(int concurrencyCount)
        {
            var tasks = Enumerable.Range(1, concurrencyCount).Select(
                i => Storage.AddOrGet(TestKey, TestValue($"value-{i}")));
            var values = await Task.WhenAll(tasks);

            var lastValue = await Storage.TryGet(TestKey);

            lastValue.Should().BeEquivalentTo(new {Value = new {Audit = new {Version = 1}}});
            values.Select(x => x.Type).Distinct().Should().HaveCount(1);
        }

        [Test]
        public async Task AddOrUpdate_returnsAddedValue()
        {
            var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added"), (_, _) => TestValue("updated"));

            value.Should().BeEquivalentTo(
                TestValue("added") with {Audit = Audit.Initial(TestDate)},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [Test]
        public async Task AddOrUpdate_returnsUpdatedValue()
        {
            await Storage.AddOrGet(TestKey, TestValue("added-1"));

            var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added-2"), (_, _) => TestValue("updated"));

            value.Should().BeEquivalentTo(
                TestValue("updated") with {Audit = new Audit(2, TestDate, TestDate)},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [TestCase(5)]
        public async Task AddOrUpdate_returnsValuesAndLastVersion_concurrently(int concurrencyCount)
        {
            var tasks = Enumerable.Range(1, concurrencyCount).Select(
                i => Storage.AddOrUpdate(TestKey, TestValue($"value-{i}")));
            var values = await Task.WhenAll(tasks);

            var lastValue = await Storage.TryGet(TestKey);

            lastValue.Should().BeEquivalentTo(new {Value = new {Audit = new {Version = concurrencyCount}}});
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
                new {Value = TestValue("value") with {Audit = Audit.Initial(TestDate)}},
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
                new {Value = TestValue("value") with {Audit = Audit.Initial(TestDate)}},
                o => o.ComparingByMembers<ValueRecord>());
        }

        [Test]
        public async Task GetKeys_returnsKeys()
        {
            await Storage.AddOrGet(TestKey, TestValue("value"));

            var value = Storage.GetKeys().ToArray();

            value.Should().BeEquivalentTo(TestKey);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            var connectionString = "mongodb://127.0.0.1:27017";
            Provider = new ServiceCollection()
                .AddStorage(b => b.AddMongo<TestKey, TestValue>().UseMongo(o => o.ConnectionString = connectionString))
                .AddSystemClock(_ => TestDate)
                .BuildServiceProvider();

            string pingContent;
            try
            {
                var mongoClient = Provider.GetRequiredService<IMongoClient>();
                var ping = await mongoClient.GetDatabase("db").RunCommandAsync(
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
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => Provider?.Dispose();

        [SetUp]
        public void Setup()
        {
            TestKey = new KeyRecord(id: $"test-{Guid.NewGuid()}", type: "test-key", content: new byte[0]);
            TestDate = DateTimeOffset.UtcNow;
        }

        [TearDown]
        public async Task TearDown() => await Storage.TryRemove(TestKey);
        
        private KeyRecord TestKey { get; set; } = default!;
        private ValueRecord TestValue(string type) => new(Type: type, Content: new byte[0]);
        private DateTimeOffset TestDate { get; set; }
        private ServiceProvider? Provider { get; set; }
        private IStorageProvider<TestValue> Storage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IStorageProvider<TestValue>>();
    }
}
