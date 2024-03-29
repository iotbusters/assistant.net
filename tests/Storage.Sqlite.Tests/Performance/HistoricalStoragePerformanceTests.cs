﻿using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Performance;

[Timeout(500)]
public class HistoricalStoragePerformanceTests
{
    [Test, Timeout(3000)]
    public async Task GetKeys_executesInTime()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.GetKeys(CancellationToken).ToArrayAsync(CancellationToken);
    }

    [Test]
    public async Task TryGet_executesInTime_existingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryGet(new(i), CancellationToken);
    }

    [Test]
    public async Task TryGet_executesInTime_noValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryGet(new(-1), CancellationToken);
    }

    [Test]
    public async Task TryGetByVersion_executesInTime_existingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryGet(new(i), version: 1, CancellationToken);
    }

    [Test]
    public async Task TryGetByVersion_executesInTime_noValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryGet(new(-1), version: 1, CancellationToken);
    }

    [Test]
    public async Task TryGetDetailed_executesInTime_existingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryGetDetailed(new(i), CancellationToken);
    }

    [Test]
    public async Task TryGetDetailed_executesInTime_noValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryGetDetailed(new(-1), CancellationToken);
    }

    [Test]
    public async Task AddOrGet_executesInTime_gettingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.AddOrGet(new(i), new TestValue(false), CancellationToken);
    }

    [Test]
    public async Task AddOrGet_executesInTime_addingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.AddOrGet(new(PrePopulatedCount + i), new TestValue(false), CancellationToken);
    }

    [Test]
    public async Task AddOrUpdate_executesInTime_updatingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.AddOrUpdate(new(i), new TestValue(false), CancellationToken);
    }

    [Test]
    public async Task AddOrUpdate_executesInTime_addingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.AddOrUpdate(new(PrePopulatedCount + i), new TestValue(false), CancellationToken);
    }

    [Test]
    public async Task TryRemove_executesInTime_existingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryRemove(new(i), CancellationToken);
    }

    [Test]
    public async Task TryRemove_executesInTime_noValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryRemove(new(-1), CancellationToken);
    }

    [Test]
    public async Task TryRemoveByUpToVersion_executesInTime_existingValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryRemove(new(MeasurementCount + i), upToVersion: 1, CancellationToken);
    }

    [Test]
    public async Task TryRemoveByUpToVersion_executesInTime_noValue()
    {
        for (var i = 0; i < MeasurementCount; i++)
            await Storage.TryRemove(new(-1), upToVersion: 1, CancellationToken);
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .Add<TestKey2, TestValue>())
            .BuildServiceProvider();

        var batchCount = 50;
        for (var i = 0; i < PrePopulatedCount / batchCount; i++)
        {
            var y = i;
            await Task.WhenAll(Enumerable.Range(1, batchCount).Select(x => Storage.AddOrGet(new(x + y), new TestValue(true))));
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (Provider == null)
            return;

        await Provider.DisposeAsync();
    }

    private const int MeasurementCount = 100;
    private const int PrePopulatedCount = 10000;

    private static CancellationToken CancellationToken => new CancellationTokenSource(1000).Token;

    private ServiceProvider? Provider { get; set; }

    private IHistoricalAdminStorage<TestKey2, TestValue> Storage => Provider!.GetRequiredService<IHistoricalAdminStorage<TestKey2, TestValue>>();
}
