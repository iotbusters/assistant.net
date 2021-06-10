using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Tests.Mocks;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Internal.Tests
{
    public class LocalStorageTests
    {
        private IServiceProvider provider = null!;

        [SetUp]
        public void Setup() =>
            provider = new ServiceCollection()
                .AddStorage(builder => builder.AddLocal<TestValue>())
                .BuildServiceProvider();

        private IStorage<TestKey, TestValue> Storage => provider.GetRequiredService<IStorage<TestKey, TestValue>>();

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
        public async Task TryGet_None_notExists()
        {
            var value = await Storage.TryGet(new TestKey("key"));
            value.Should().Be(new None<TestValue>());
        }

        [Test]
        public async Task TryGet_Some_exists()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.TryGet(new TestKey("key"));
            value.Should().Be(new Some<TestValue>(new TestValue("value")));
        }

        [Test]
        public async Task TryRemove_None_notExists()
        {
            var value = await Storage.TryRemove(new TestKey("key"));
            value.Should().Be(new None<TestValue>());
        }

        [Test]
        public async Task TryRemove_Some_exists()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.TryRemove(new TestKey("key"));
            value.Should().Be(new Some<TestValue>(new TestValue("value")));
        }

        [Test]
        public async Task GetKeys_listOfKeys()
        {
            await Storage.AddOrGet(new TestKey("key"), new TestValue("value"));

            var value = await Storage.GetKeys().AsEnumerableAsync();
            value.Should().BeEquivalentTo(new TestKey("key"));
        }
    }
}