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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal
{
    public class MongoHistoricalStorageProviderIntegrationTests
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
            var tasks = Enumerable.Range(1, concurrencyCount).Select(i =>
                Storage.AddOrGet(TestKey, TestValue($"value-{i}")));
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
            var tasks = Enumerable.Range(1, concurrencyCount).Select(i =>
                Storage.AddOrUpdate(TestKey, TestValue($"value-{i}")));
            var values = await Task.WhenAll(tasks);

            var lastValue = await Storage.TryGet(TestKey);

            lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit(concurrencyCount)}});
            values.Should().BeEquivalentTo(Enumerable.Range(1, concurrencyCount).Select(i => new {Type = $"value-{i}"}));
        }

        [TestCase(1000), Ignore("Manual run only")]
        public async Task AddOrUpdate_returnsUpdatedValueInTime_keysAndVersions(int count)
        {
            foreach (var i in Enumerable.Range(1, count))
            {
                await Storage.AddOrUpdate(new KeyRecord(i.ToString(), "type", new byte[0]), TestValue($"{i}-1"));
                await Storage.AddOrUpdate(TestKey, TestValue($"value-{i}"));
            }

            var watch = Stopwatch.StartNew();
            var value = await Storage.AddOrUpdate(TestKey, TestValue("value-X"));
            watch.Stop();
            watch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(0.1));

            value.Should().BeEquivalentTo(
                TestValue("value-X") with { Audit = Audit(count + 1) },
                o => o.ComparingByMembers<ValueRecord>());
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
        public async Task TryGetByVersion_returnsNone_notExists()
        {
            var value = await Storage.TryGet(TestKey, version: 1);

            value.Should().BeEquivalentTo(new None<ValueRecord>());
        }

        [Test]
        public async Task TryGetByVersion_returnsSome_exists()
        {
            await Storage.AddOrGet(TestKey, TestValue("value"));

            var value = await Storage.TryGet(TestKey, version: 1);

            value.Should().BeEquivalentTo(
                new { Value = TestValue("value") with { Audit = Audit() } },
                o => o.ComparingByMembers<ValueRecord>());
        }

        [TestCase(1000), Ignore("Manual run only")]
        public async Task TryGet_returnsValueInTime_keysAndVersions(int count)
        {
            foreach (var i in Enumerable.Range(1, count))
            {
                await Storage.AddOrUpdate(new KeyRecord(i.ToString(), "type", new byte[0]), TestValue($"{i}-1"));
                await Storage.AddOrUpdate(TestKey, TestValue($"value-{i}"));
            }

            var watch = Stopwatch.StartNew();
            var value = await Storage.TryGet(TestKey);
            watch.Stop();
            watch.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(0.1));

            value.Should().BeEquivalentTo(
                TestValue($"value-{count}") with { Audit = Audit(count) },
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
        public async Task TryRemove_doesNotLostVersions_AddOrUpdateConcurrently()
        {
            await Storage.AddOrGet(TestKey, TestValue("value-1"));

            var removeTask = Storage.TryRemove(TestKey);
            await Task.WhenAll(
                removeTask,
                Storage.AddOrUpdate(TestKey, TestValue("value-2")),
                Storage.AddOrUpdate(TestKey, TestValue("value-3")),
                Storage.AddOrUpdate(TestKey, TestValue("value-4")),
                Storage.AddOrUpdate(TestKey, TestValue("value-5")));

            var value = await Storage.TryGet(TestKey);
            (removeTask.Result.GetValueOrDefault()?.Audit.Version ?? 0
             + value.GetValueOrDefault()?.Audit.Version ?? 0).Should().Be(5);
        }

        [Test]
        public async Task TryRemoveByVersion_returnsNone_notExists()
        {
            var count = await Storage.TryRemove(TestKey, upToVersion: 1);

            count.Should().Be(0L);
        }

        [Test]
        public async Task TryRemoveByVersion_returnsSome_exists()
        {
            foreach (var i in Enumerable.Range(1, 5))
                await Storage.AddOrUpdate(TestKey, TestValue($"value-{i}"));

            var count = await Storage.TryRemove(TestKey, upToVersion: 4);

            count.Should().Be(4);
            var value5 = await Storage.TryGet(TestKey, version: 5);
            value5.Should().BeEquivalentTo(
                new {Value = TestValue("value-5") with {Audit = Audit(5)}},
                o => o.ComparingByMembers<ValueRecord>());
            var value4 = await Storage.TryGet(TestKey, version: 4);
            value4.Should().BeEquivalentTo(new None<ValueRecord>());
            var value1 = await Storage.TryGet(TestKey, version: 1);
            value1.Should().BeEquivalentTo(new None<ValueRecord>());
        }

        [Test]
        public async Task GetKeys_returnsKeys()
        {
            await MongoClient.DropDatabaseAsync(MongoNames.DatabaseName);
            await Storage.AddOrGet(TestKey, TestValue("value"));

            var value = Storage.GetKeys().ToArray();

            value.Should().BeEquivalentTo(TestKey);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            var connectionString = "mongodb://127.0.0.1:27017";
            Provider = new ServiceCollection()
                .AddStorage(b => b.AddMongoHistorical<TestKey, TestValue>().UseMongo(o => o.ConnectionString = connectionString))
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
        private Audit Audit(int version = 1) => new(new Audit(TestCorrelationId, TestUser).Details, version) {Created = TestDate};
        private KeyRecord TestKey { get; set; } = default!;
        private string TestCorrelationId { get; set; } = default!;
        private string TestUser { get; set; } = default!;
        private DateTimeOffset TestDate { get; set; }
        private ServiceProvider Provider { get; set; } = default!;
        private IMongoClient MongoClient => Provider!.CreateScope().ServiceProvider.GetRequiredService<IMongoClientFactory>().CreateClient();
        private IHistoricalStorageProvider<TestValue> Storage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IHistoricalStorageProvider<TestValue>>();
    }
}
