
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Utils;
using Assistant.Net.Storage.Tests.Mocks;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Tests
{
    public class StorageTests
    {
        private IServiceProvider provider = null!;

        private IStorage<TestKey, TestValue> Storage => provider.GetRequiredService<IStorage<TestKey, TestValue>>();

        [SetUp]
        public void Setup()
        {
            provider = new ServiceCollection()
                .AddStorage(builder => builder.AddLocal<TestValue>())
                .BuildServiceProvider();
        }

        [Test]
        public void GetService_null_StorageOfUnknownValue()
        {
            var storage = provider.GetService<IStorage<string>>();

            storage.Should().BeNull();
        }

        [Test]
        public void GetService_throw_StorageOfUnknownValue()
        {
            provider.Invoking(x => x.GetService<IStorage<string, string>>())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Storage of String wasn't properly configured.");
        }

        [Test]
        public async Task TryGet_Some_FromAnotherStorageOfTheSameValue()
        {
            var storage1 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();
            var storage2 = provider.GetRequiredService<IStorage<string, TestValue>>();

            await storage1.AddOrGet(new TestKey("key"), new TestValue("value"));
            var value = await storage2.TryGet($"{nameof(TestKey)}-{new TestKey("key").GetSha1()}");

            value.Should().Be(Option.Some(new TestValue("value")));
        }

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
    }
}