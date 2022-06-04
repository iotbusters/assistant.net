using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Options;
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
        .AddStorage(b => b
            .UseSqlite("Data Source=:memory:")
            .AddSqlite<TestKey, object>()
            .AddSqlitePartitioned<TestKey, object>()
            .AddSqliteHistorical<TestKey, object>())
        .BuildServiceProvider();

    private static IServiceProvider SingleProvider => new ServiceCollection()
        .AddSystemClock()
        .AddStorage(b => b
            .UseSqlite("Data Source=:memory:")
            .UseSqliteSingleProvider()
            .AddSingle<TestKey, object>()
            .AddSinglePartitioned<TestKey, object>()
            .AddSingleHistorical<TestKey, object>())
        .BuildServiceProvider();


    private static IServiceProvider NamedProvider => new ServiceCollection()
        .AddSystemClock()
        .AddStorage(b => b.UseSqlite("Data Source=:memory:").AddSqlite<TestKey, object>())
        .ConfigureStorage("1", b => b.UseSqlite("Data Source=:memory:").AddSqlitePartitioned<TestKey, object>())
        .ConfigureStorage("2", b => b.UseSqlite("Data Source=:memory:").AddSqliteHistorical<TestKey, object>())
        .BuildServiceProvider();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithSingleProviderDefined_UsingSingleStorageProvider() =>
        SingleProvider.GetService<INamedOptions<StorageOptions>>()?.Value.SingleProvider
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIStorage_returnsInstance_UsingSingleStorageProvider() =>
        SingleProvider.GetService<IStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithSinglePartitionedProviderDefined_UsingSingleStorageProvider() =>
        SingleProvider.GetService<INamedOptions<StorageOptions>>()?.Value.SinglePartitionedProvider
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIPartitionedStorage_returnsInstance_UsingSingleStorageProvider() =>
        SingleProvider.GetService<IPartitionedStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithSingleHistoricalProviderDefined_UsingSingleStorageProvider() =>
        SingleProvider.GetService<INamedOptions<StorageOptions>>()?.Value.SingleHistoricalProvider
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIHistoricalStorage_returnsInstance_UsingSingleStorageProvider() =>
        SingleProvider.GetService<IHistoricalStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithProviderPopulated_Registered() =>
        Provider.GetService<INamedOptions<StorageOptions>>()?.Value.Providers.Keys
            .Should().BeEquivalentTo(new[] { typeof(object) });

    [Test]
    public void GetServiceOfIStorage_returnsInstance_Registered() =>
        Provider.GetService<IStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithPartitionedProviderPopulated_Registered() =>
        Provider.GetService<INamedOptions<StorageOptions>>()?.Value.PartitionedProviders.Keys
            .Should().BeEquivalentTo(new[] { typeof(object) });

    [Test]
    public void GetServiceOfIPartitionedStorage_returnsInstance_Registered() =>
        Provider.GetService<IPartitionedStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfStorageOptions_returnsInstanceWithHistoricalProviderPopulated_Registered() =>
        Provider.GetService<INamedOptions<StorageOptions>>()?.Value.HistoricalProviders.Keys
            .Should().BeEquivalentTo(new[] { typeof(object) });

    [Test]
    public void GetServiceOfIHistoricalStorage_returnsInstance_Registered() =>
        Provider.GetService<IHistoricalStorage<TestKey, object>>()
            .Should().NotBeNull();

    [Test]
    public void GetServiceOfIStorage_throwsException_Unregistered() =>
        Provider.Invoking(x => x.GetService<IStorage<object, DateTime>>())
            .Should().Throw<ArgumentException>()
            .WithMessage("Storage(DateTime) wasn't properly configured.");

    [Test]
    public void GetServiceOfIPartitionedStorage_throwsException_Unregistered() =>
        Provider.Invoking(x => x.GetService<IPartitionedStorage<object, DateTime>>())
            .Should().Throw<ArgumentException>()
            .WithMessage("PartitionedStorage(DateTime) wasn't properly configured.");

    [Test]
    public void GetServiceOfIHistoricalStorage_throwsException_Unregistered() =>
        Provider.Invoking(x => x.GetService<IHistoricalStorage<object, DateTime>>())
            .Should().Throw<ArgumentException>()
            .WithMessage("HistoricalStorage(DateTime) wasn't properly configured.");

    [Test]
    public void GetServiceOfStorages_returnsIHistoricalStorageInstanceAndThrowsTheRest_namedScopeDefault()
    {
        var provider = NamedProvider.CreateAsyncScope().ServiceProvider;
        provider.GetService<IStorage<TestKey, object>>().Should().NotBeNull();
        provider.Invoking(x => x.GetService<IPartitionedStorage<TestKey, object>>()).Should().Throw<ArgumentException>();
        provider.Invoking(x => x.GetService<IHistoricalStorage<TestKey, object>>()).Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetServiceOfStorages_returnsIPartitionedStorageInstanceAndThrowsTheRest_namedScope1()
    {
        var provider = NamedProvider.CreateAsyncScopeWithNamedOptionContext("1").ServiceProvider;
        provider.Invoking(x => x.GetService<IStorage<TestKey, object>>()).Should().Throw<ArgumentException>();
        provider.GetService<IPartitionedStorage<TestKey, object>>().Should().NotBeNull();
        provider.Invoking(x => x.GetService<IHistoricalStorage<TestKey, object>>()).Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetServiceOfStorages_returnsIStorageInstanceAndThrowsTheRest_namedScope2()
    {
        var provider = NamedProvider.CreateAsyncScopeWithNamedOptionContext("2").ServiceProvider;
        provider.Invoking(x => x.GetService<IStorage<TestKey, object>>()).Should().Throw<ArgumentException>();
        provider.Invoking(x => x.GetService<IPartitionedStorage<TestKey, object>>()).Should().Throw<ArgumentException>();
        provider.GetService<IHistoricalStorage<TestKey, object>>().Should().NotBeNull();
    }
}
