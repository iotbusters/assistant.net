using System;
using System.Threading.Tasks;
using Assistant.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Tests.Mocks;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private static IServiceProvider Provider => new ServiceCollection()
            .AddStorage(b => b.AddLocal<TestKey, object>())
            .BuildServiceProvider();

        [Test]
        public void GetServiceOfIKeyConverterOfString_resolvesObject()
        {
            Provider.GetService<IKeyConverter<StoreKey>>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfIKeyConverterOfObject_resolvesObject()
        {
            Provider.GetService<IKeyConverter<object>>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfIStorage_null_StorageOfUnknownValue()
        {
            Provider.GetService<IStorageProvider<DateTime>>()
                .Should().BeNull();
        }

        [Test]
        public void GetServiceOfIStorage_throw_StorageOfUnknownValue()
        {
            Provider.Invoking(x => x.GetService<IStorage<object, DateTime>>())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Storage of 'DateTime' wasn't properly configured.");
        }

        [Test]
        public async Task TryGet_Some_FromStorageProviderOfTheSameValue()
        {
            var provider = new ServiceCollection()
                .AddSingleton<IStorageProvider<TestValue>>(new TestStorage<TestValue>())
                .AddSerializer(b => b.AddJsonType<TestKey>().AddJsonType<TestValue>())
                .AddStorage(b => { })
                .BuildServiceProvider();

            var storage1 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();
            var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

            var key = new TestKey("key");
            await storage1.AddOrGet(key, new TestValue("value"));
            var value = await storage2.TryGet(key);

            value.Should().Be(Option.Some(new TestValue("value")));
        }

        [Test]
        public async Task TryGet_None_FromStorageOfAnotherValue()
        {
            var provider = new ServiceCollection()
                .AddSingleton<IStorageProvider<object>>(new TestStorage<object>())
                .AddSingleton<IStorageProvider<string>>(new TestStorage<string>())
                .AddSerializer(b => b.AddJsonType<TestKey>().AddJsonType<object>().AddJsonType<string>())
                .AddStorage(b => { })
                .BuildServiceProvider();

            var storage1 = provider.GetRequiredService<IStorage<TestKey, object>>();
            var storage2 = provider.GetRequiredService<IStorage<TestKey, string>>();

            var key = new TestKey("key");
            await storage1.AddOrGet(key, "value");
            var value = await storage2.TryGet(key);

            value.Should().Be((Option<string>)Option.None);
        }
    }
}