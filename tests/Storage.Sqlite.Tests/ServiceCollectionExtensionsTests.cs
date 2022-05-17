using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Assistant.Net.Storage.Sqlite.Tests;

public class ServiceCollectionExtensionsTests
{
    private static IServiceProvider Provider => new ServiceCollection()
        .AddSystemClock()
        .AddStorage(b => b.UseSqlite("Data Source=:memory:").AddSqlite<TestKey, object>().AddSqlitePartitioned<TestKey, object>())
        .BuildServiceProvider();

    [Test]
    public void GetServiceOfIStorageProvider_returnsInstance_StorageProviderOfRegisteredValue()
    {
        Provider.GetService<IStorageProvider<object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfIStorage_returnsInstance_StorageOfRegisteredValue()
    {
        Provider.GetService<IStorage<TestKey, object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfIPartitionedStorageProvider_returnsInstance_PartitionedStorageProviderOfRegisteredValue()
    {
        Provider.GetService<IPartitionedStorageProvider<object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfIPartitionedStorage_returnsInstance_StorageProviderOfRegisteredValue()
    {
        Provider.GetService<IPartitionedStorage<TestKey, object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfIStorageProvider_returnsNull_StorageOfUnknownValue()
    {
        Provider.GetService<IStorageProvider<DateTime>>()
            .Should().BeNull();
    }

    [Test]
    public void GetServiceOfIStorage_throwsException_StorageOfUnknownValue()
    {
        Provider.Invoking(x => x.GetService<IStorage<object, DateTime>>())
            .Should().Throw<ArgumentException>()
            .WithMessage("Storage of 'DateTime' wasn't properly configured.");
    }

    [Test]
    public void GetServiceOfIPartitionedStorageProvider_returnsNull_StorageOfUnknownValue()
    {
        Provider.GetService<IPartitionedStorageProvider<DateTime>>()
            .Should().BeNull();
    }

    [Test]
    public void GetServiceOfIPartitionedStorage_throwsException_PartitionedStorageOfUnknownValue()
    {
        Provider.Invoking(x => x.GetService<IPartitionedStorage<object, DateTime>>())
            .Should().Throw<ArgumentException>()
            .WithMessage("Partitioned storage of 'DateTime' wasn't properly configured.");
    }
}
