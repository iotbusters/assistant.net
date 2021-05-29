using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Tests.Mocks;
using Assistant.Net.Storage.Utils;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddStorage_registersServiceDescriptors()
        {
            var services = new ServiceCollection().AddStorage(_ => { });

            services.Should().BeEquivalentTo(
                new
                {
                    ServiceType = typeof(IStorage<,>),
                    ImplementationType = new { Name = "Storage`2" }
                },
                new
                {
                    ServiceType = typeof(IKeyConverter<string>),
                    ImplementationType = new { Name = "StringKeyConverter" }
                },
                new
                {
                    ServiceType = typeof(IKeyConverter<>),
                    ImplementationType = new { Name = "KeyConverter`1" }
                });
        }

        [Test]
        public void GetServiceOfIKeyConverterOfString_resolvesObject()
        {
            var provider = new ServiceCollection().AddStorage(_ => { }).BuildServiceProvider();

            provider.GetService<IKeyConverter<string>>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfIKeyConverterOfObject_resolvesObject()
        {
            var provider = new ServiceCollection().AddStorage(_ => { }).BuildServiceProvider();

            provider.GetService<IKeyConverter<object>>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfIStorage_null_StorageOfUnknownValue()
        {
            var provider = new ServiceCollection().AddStorage(_ => { }).BuildServiceProvider();

            provider.GetService<IStorage<object>>()
                .Should().BeNull();
        }

        [Test]
        public void GetServiceOfIStorage_throw_StorageOfUnknownValue()
        {
            var provider = new ServiceCollection().AddStorage(_ => { }).BuildServiceProvider();

            provider.Invoking(x => x.GetService<IStorage<object, object>>())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Storage of 'Object' wasn't properly configured.");
        }

        [Test]
        public async Task TryGet_Some_FromAnotherStorageOfTheSameValue()
        {
            var testStorage = new TestStorage();
            var provider = new ServiceCollection()
                .AddStorage(b => b.Services.AddSingleton<IStorage<object>>(testStorage))
                .BuildServiceProvider();

            var storage1 = provider.GetRequiredService<IStorage<TestKey, object>>();
            var storage2 = provider.GetRequiredService<IStorage<string, object>>();

            await storage1.AddOrGet(new TestKey("key"), new TestValue("value"));
            var key = $"{nameof(TestKey)}-{new TestKey("key").GetSha1()}";
            var value = await storage2.TryGet(key);

            value.Should().Be(Option.Some<object>(new TestValue("value")));
        }

    }
}