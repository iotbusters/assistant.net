using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Options;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Assistant.Net.Storage.Sqlite.Tests;

public class ServiceCollectionExtensionsTests
{
    private const string ConnectionString = "Data Source=:memory:";

    private static IServiceProvider Provider => new ServiceCollection()
        .AddStorage(b => b
            .UseSqlite(ConnectionString)
            .Add<TestKey, object>())
        .BuildServiceProvider();


    private static IServiceProvider NamedProvider => new ServiceCollection()
        .AddStorage(b => b.UseSqlite(ConnectionString).Add<TestKey, object>())
        .AddStorage("1", b => b.UseSqlite(ConnectionString))
        .AddStorage("2", b => b.Add<TestKey, object>())
        .BuildServiceProvider();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithStorageProviderFactoryDefined() =>
        Provider.GetService<INamedOptions<StorageOptions>>()?.Value.StorageProviderFactory
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIStorage_returnsInstance() =>
        Provider.GetService<IStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithPartitionedStorageProviderFactoryDefined() =>
        Provider.GetService<INamedOptions<StorageOptions>>()?.Value.PartitionedStorageProviderFactory
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIPartitionedStorage_returnsInstance() =>
        Provider.GetService<IPartitionedStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithHistoricalStorageProviderFactoryDefined() =>
        Provider.GetService<INamedOptions<StorageOptions>>()?.Value.HistoricalStorageProviderFactory
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIHistoricalStorage_returnsInstance() =>
        Provider.GetService<IHistoricalStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithRegistrationsPopulated_Registered() =>
        Provider.GetService<INamedOptions<StorageOptions>>()?.Value.Registrations
            .Should().BeEquivalentTo(new[] { typeof(object) });

    [Test]
    public void GetServiceOfIStorage_returnsInstance_Registered() =>
        Provider.GetService<IStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIPartitionedStorage_returnsInstance_Registered() =>
        Provider.GetService<IPartitionedStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIHistoricalStorage_returnsInstance_Registered() =>
        Provider.GetService<IHistoricalStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIStorage_throwsException_Unregistered() =>
        Provider.Invoking(x => x.GetService<IStorage<object, DateTime>>())
            .Should().Throw<StoringTypeNotRegisteredException>();

    [Test]
    public void GetServiceOfIPartitionedStorage_throwsException_Unregistered() =>
        Provider.Invoking(x => x.GetService<IPartitionedStorage<object, DateTime>>())
            .Should().Throw<StoringTypeNotRegisteredException>();

    [Test]
    public void GetServiceOfIHistoricalStorage_throwsException_Unregistered() =>
        Provider.Invoking(x => x.GetService<IHistoricalStorage<object, DateTime>>())
            .Should().Throw<StoringTypeNotRegisteredException>();

    [Test]
    public void GetServiceOfStorages_throwsException_unnamedScope()
    {
        var provider = NamedProvider.CreateAsyncScope().ServiceProvider;
        provider.GetService<IStorage<TestKey, object>>().Should().NotBeNull();
        provider.GetService<IPartitionedStorage<TestKey, object>>().Should().NotBeNull();
        provider.GetService<IHistoricalStorage<TestKey, object>>().Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfStorages_throwsException_namedScope1()
    {
        var provider = NamedProvider.CreateAsyncScopeWithNamedOptionContext("1").ServiceProvider;
        provider.Invoking(x => x.GetService<IStorage<TestKey, object>>()).Should().Throw<StoringTypeNotRegisteredException>();
        provider.Invoking(x => x.GetService<IPartitionedStorage<TestKey, object>>()).Should().Throw<StoringTypeNotRegisteredException>();
        provider.Invoking(x => x.GetService<IHistoricalStorage<TestKey, object>>()).Should().Throw<StoringTypeNotRegisteredException>();
    }

    [Test]
    public void GetServiceOfStorages_throwsException_namedScope2()
    {
        var provider = NamedProvider.CreateAsyncScopeWithNamedOptionContext("2").ServiceProvider;
        provider.Invoking(x => x.GetService<IStorage<TestKey, object>>()).Should().Throw<StorageProviderNotRegisteredException>();
        provider.Invoking(x => x.GetService<IPartitionedStorage<TestKey, object>>()).Should().Throw<StorageProviderNotRegisteredException>();
        provider.Invoking(x => x.GetService<IHistoricalStorage<TestKey, object>>()).Should().Throw<StorageProviderNotRegisteredException>();
    }
}
