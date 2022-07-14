using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Internal;

public class SqliteStorageProviderIntegrationTests
{
    [Test]
    public async Task AddOrGet_returnsAddedValue_notExists()
    {
        var value = await Storage.AddOrGet(TestKey, TestValue(version: 2));

        value.Should().BeEquivalentTo(TestValue(version: 2));
    }

    [Test]
    public async Task AddOrGet_returnsExistingValue_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.AddOrGet(TestKey, TestValue());

        value.Should().BeEquivalentTo(TestValue());
    }

    [TestCase(5)]
    public async Task AddOrGet_returnsValuesAndInitialVersion_concurrently(int concurrencyCount)
    {
        var tasks = Enumerable.Range(1, concurrencyCount).Select(_ => Storage.AddOrGet(TestKey, TestValue()));
        var values = await Task.WhenAll(tasks);

        var lastValue = await Storage.TryGet(TestKey);

        lastValue.Should().BeEquivalentTo(TestValue().AsOption());
        values.Select(x => x.Audit.Version).Distinct().Should().HaveCount(1);
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedValue()
    {
        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue(version:2), (_, _) => TestValue(version: 3));

        value.Should().BeEquivalentTo(TestValue(version: 2));
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedValue()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue(version: 2), (_, _) => TestValue(version: 3));

        value.Should().BeEquivalentTo(TestValue(version: 3));
    }

    [TestCase(5)]
    public async Task AddOrUpdate_returnsValuesAndLastVersion_concurrently(int concurrencyCount)
    {
        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            i => Storage.AddOrUpdate(TestKey, TestValue(version: i)));
        var values = await Task.WhenAll(tasks);

        var lastValue = await Storage.TryGet(TestKey);

        lastValue.Should().BeEquivalentTo(TestValue(version: concurrencyCount).AsOption());
        values.Should().BeEquivalentTo(Enumerable.Range(1, concurrencyCount).Select(TestValue));
    }

    [Test]
    public async Task TryGet_returnsNone_notExists()
    {
        var value = await Storage.TryGet(TestKey);

        value.Should().Be(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGet_returnsSome_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.TryGet(TestKey);

        value.Should().BeEquivalentTo(TestValue().AsOption());
    }

    [Test]
    public async Task TryRemove_returnsNone_notExists()
    {
        var value = await Storage.TryRemove(TestKey);

        value.Should().Be(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsSome_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.TryRemove(TestKey);

        value.Should().BeEquivalentTo(TestValue().AsOption());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.GetKeys(_ => true).ToArrayAsync();

        value.Should().BeEquivalentTo(new[] {TestKey});
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageProviderOfTheSameValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .AddSqlite<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();
        var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.AddOrGet(key, new TestValue(true));
        var value = await storage2.TryGet(key);

        value.Should().Be(new TestValue(true).AsOption());
    }

    [Test]
    public async Task TryGet_returnsNone_FromStorageOfAnotherValue()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .AddSqlite<TestKey, TestBase>()
                .AddSqlite<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.AddOrGet(key, new TestValue(true));
        var value = await storage2.TryGet(key);

        value.Should().Be(new None<TestValue>());
    }

    [Test]
    public async Task TryGet_returnsSome_FromStorageUsedAdding()
    {
        var provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .AddSqlite<TestKey, TestBase>()
                .AddSqlite<TestKey, TestValue>())
            .BuildServiceProvider();

        var storage1 = provider.GetRequiredService<IStorage<TestKey, TestBase>>();
        var storage2 = provider.GetRequiredService<IStorage<TestKey, TestValue>>();

        var key = new TestKey(true);
        await storage1.AddOrGet(key, new TestValue(true));
        await storage2.AddOrGet(key, new TestValue(false));
        var value1 = await storage1.TryGet(key);
        var value2 = await storage2.TryGet(key);

        value1.Should().Be(new TestValue(true).AsOption<TestBase>());
        value2.Should().Be(new TestValue(false).AsOption());
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(SetupSqlite.ConnectionString)
                .AddSqlite<TestKey, TestValue>())
            .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
            .AddSystemClock(_ => TestDate)
            .BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider?.Dispose();

    [SetUp]
    public async Task Cleanup()
    {
        var dbContext = await Provider!.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync(CancellationToken);
        dbContext.StorageKeys.RemoveRange(dbContext.StorageKeys);
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private static CancellationToken CancellationToken => new CancellationTokenSource(100).Token;
    private ValueRecord TestValue(int version = 1) => new(Content: Array.Empty<byte>(), Audit(version));
    private Audit Audit(int version = 1) => new(version) {CorrelationId = TestCorrelationId, User = TestUser, Created = TestDate};
    private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", valueType: nameof(Mocks.TestValue), content: Array.Empty<byte>());
    private string TestCorrelationId { get; set; } = Guid.NewGuid().ToString();
    private string TestUser { get; set; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
    private ServiceProvider? Provider { get; set; }

    private IStorageProvider<TestValue> Storage => (IStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value.Providers[typeof(TestValue)].Create(Provider!);
}
