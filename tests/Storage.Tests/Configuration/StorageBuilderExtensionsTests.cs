using System;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Storage.Configuration.Tests
{
    public class StorageBuilderExtensionsTests
    {
        private ServiceCollection services = null!;

        [SetUp]
        public void Setup() => services = new ServiceCollection();

        [Test]
        public void AddLocalOfType_registersLocalStorageOfType()
        {
            new StorageBuilder(services).AddLocal<object>();

            services.Should().BeEquivalentTo(new
            {
                ServiceType = typeof(IStorage<object>),
                ImplementationType = new { Name = "LocalStorage`1" }
            });
        }

        [Test]
        public void AddLocalAny_registersLocalStorageOfAny()
        {
            new StorageBuilder(services).AddLocalAny();

            services.Should().BeEquivalentTo(new
            {
                ServiceType = typeof(IStorage<>),
                ImplementationType = new { Name = "LocalStorage`1" }
            });
        }

    }
}