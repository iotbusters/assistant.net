using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Tests.Internal
{
    public class HistoricalLocalStorageTests
    {
        [Test]
        public async Task AddOrGet_addsAndGets()
        {
            var addedValue = await Storage.AddOrGet(new TestKey("key"), new TestValue("value-1"));
            addedValue.Should().Be(new TestValue("value-1"));

            var existingValue = await Storage.AddOrGet(new TestKey("key"), new TestValue("value-2"));
            existingValue.Should().Be(new TestValue("value-1"));
        }

        [Test]
        public async Task AddOrUpdate_addsAndUpdates()
        {
            var addedValue = await Storage.AddOrUpdate(new TestKey("key"), new TestValue("value-1"));
            addedValue.Should().Be(new TestValue("value-1"));

            var existingValue = await Storage.AddOrUpdate(new TestKey("key"), new TestValue("value-2"));
            existingValue.Should().Be(new TestValue("value-2"));
        }

        [Test]
        public async Task TryGet_returnsNone_notExists()
        {
            var value = await Storage.TryGet(new TestKey("key"));
            value.Should().Be(new None<TestValue>());
        }

        [Test]
        public async Task TryGet_returnsSome_exists()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.TryGet(new TestKey("key"));
            value.Should().Be(new Some<TestValue>(new TestValue("value")));
        }

        [Test]
        public async Task TryGetByVersion_returnsNone_notExists()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.TryGet(new TestKey("key"), version: 2);
            value.Should().Be(new None<TestValue>());
        }

        [Test]
        public async Task TryGetByVersion_returnsSome_exists()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.TryGet(new TestKey("key"), version: 1);
            value.Should().Be(new Some<TestValue>(new TestValue("value")));
        }

        [Test]
        public async Task TryRemove_returnsNone_notExists()
        {
            var value = await Storage.TryRemove(new TestKey("key"));
            value.Should().Be(new None<TestValue>());
        }

        [Test]
        public async Task TryRemove_returnsSome_exists()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.TryRemove(new TestKey("key"));
            value.Should().Be(new Some<TestValue>(new TestValue("value")));
        }

        [Test]
        public async Task GetKeys_returnsListOfKeys()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.GetKeys().AsEnumerableAsync();
            value.Should().BeEquivalentTo(new TestKey("key"));
        }

        [Test]
        public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
        {
            var provider = new ServiceCollection()
                .AddStorage(b => b.AddLocal<TestKey, TestValue>())
                .BuildServiceProvider();

            var storage1 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();
            var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

            var key = new TestKey("key");
            await storage1.AddOrGet(key, new TestValue("value"));
            var value = await storage2.TryGet(key);

            value.Should().Be(Option.Some(new TestValue("value")));
        }

        [Test]
        public async Task TryGet_returnsNone_FromStorageOfAnotherValue()
        {
            var provider = new ServiceCollection()
                .AddStorage(b => b.AddLocal<TestKey, object>().AddLocal<TestKey, string>())
                .BuildServiceProvider();

            var storage1 = provider.GetRequiredService<IStorage<TestKey, object>>();
            var storage2 = provider.GetRequiredService<IStorage<TestKey, string>>();

            var key = new TestKey("key");
            await storage1.AddOrGet(key, "value");
            var value = await storage2.TryGet(key);

            value.Should().Be((Option<string>)Option.None);
        }

        [SetUp]
        public void Setup() =>
            Provider = new ServiceCollection()
                .AddSystemClock()
                .AddStorage(b => b.AddHistoricalLocal<TestKey, TestValue>())
                .BuildServiceProvider();

        private IServiceProvider Provider { get; set; } = null!;

        private IHistoricalAdminStorage<TestKey, TestValue> Storage =>
            Provider.CreateScope().ServiceProvider.GetRequiredService<IHistoricalAdminStorage<TestKey, TestValue>>();
    }
}
