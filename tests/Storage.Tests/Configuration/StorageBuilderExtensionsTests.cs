using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;

namespace Assistant.Net.Storage.Tests.Configuration;

public class StorageBuilderExtensionsTests
{
    private ServiceCollection services = null!;

    [SetUp]
    public void Setup() => services = new ServiceCollection();

    [Test]
    public void AddLocalOfType_registersLocalStorageOfType()
    {
        new StorageBuilder(services).AddLocal<object, object>();

        services.Should().ContainEquivalentOf(new
        {
            ServiceType = typeof(IStorageProvider<object>),
            ImplementationType = new { Name = "LocalStorageProvider`1" }
        });
    }

    [Test]
    public void AddLocalAny_registersLocalStorageOfAny()
    {
        new StorageBuilder(services).AddLocalAny();

        services.Should().ContainEquivalentOf(new
        {
            ServiceType = typeof(IStorageProvider<>),
            ImplementationType = new { Name = "LocalStorageProvider`1" }
        });
    }
}
