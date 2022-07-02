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

        value.Should().BeEquivalentTo(new {Type = nameof(Mocks.TestValue), Audit = Audit(version: 2)});
    }

    [Test]
    public async Task AddOrGet_returnsExistingValue_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.AddOrGet(TestKey, TestValue());

        value.Should().BeEquivalentTo(TestValue(), o => o.ComparingByMembers<ValueRecord>());
    }

    [TestCase(5)]
    public async Task AddOrGet_returnsValuesAndInitialVersion_concurrently(int concurrencyCount)
    {
        var tasks = Enumerable.Range(1, concurrencyCount).Select(_ => Storage.AddOrGet(TestKey, TestValue()));
        var values = await Task.WhenAll(tasks);

        var lastValue = await Storage.TryGet(TestKey);

        lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit()}});
        values.Select(x => x.Type).Distinct().Should().HaveCount(1);
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedValue()
    {
        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue(version:2), (_, _) => TestValue(version: 3));

        value.Should().BeEquivalentTo(TestValue(version: 2), o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedValue()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue(version: 2), (_, _) => TestValue(version: 3));

        value.Should().BeEquivalentTo(TestValue(version: 3), o => o.ComparingByMembers<ValueRecord>());
    }

    [TestCase(5)]
    public async Task AddOrUpdate_returnsValuesAndLastVersion_concurrently(int concurrencyCount)
    {
        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            i => Storage.AddOrUpdate(TestKey, TestValue(version: i)));
        var values = await Task.WhenAll(tasks);

        var lastValue = await Storage.TryGet(TestKey);

        lastValue.Should().BeEquivalentTo(new {Value = TestValue(version: concurrencyCount) });
        values.Should().BeEquivalentTo(Enumerable.Range(1, concurrencyCount).Select(TestValue));
    }

    [Test]
    public async Task TryGet_returnsNone_notExists()
    {
        var value = await Storage.TryGet(TestKey);

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryGet_returnsSome_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.TryGet(TestKey);

        value.Should().BeEquivalentTo(new {Value = TestValue()}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsNone_notExists()
    {
        var value = await Storage.TryRemove(TestKey);

        value.Should().BeEquivalentTo(new None<ValueRecord>());
    }

    [Test]
    public async Task TryRemove_returnsSome_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = await Storage.TryRemove(TestKey);

        value.Should().BeEquivalentTo(new {Value = TestValue()}, o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.AddOrGet(TestKey, TestValue());

        var value = Storage.GetKeys().ToArray();

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

        value.Should().Be(Option.Some(new TestValue(true)));
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

        value.Should().Be((Option<TestValue>)Option.None);
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

        value1.Should().Be(Option.Some<TestBase>(new TestValue(true)));
        value2.Should().Be(Option.Some(new TestValue(false)));
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

    [TearDown]
    public async Task TearDown()
    {
        var dbContext = await Provider!.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync(CancellationToken);
        dbContext.StorageKeys.RemoveRange(dbContext.StorageKeys);
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private static CancellationToken CancellationToken => new CancellationTokenSource(100).Token;
    private ValueRecord TestValue(int version = 1) => new(Type: nameof(Mocks.TestValue), Content: Array.Empty<byte>(), Audit(version));
    private Audit Audit(int version = 1) => new(version) {CorrelationId = TestCorrelationId, User = TestUser, Created = TestDate};
    private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", content: Array.Empty<byte>(), valueType: nameof(Mocks.TestValue));
    private string TestCorrelationId { get; set; } = Guid.NewGuid().ToString();
    private string TestUser { get; set; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
    private ServiceProvider? Provider { get; set; }

    private IStorageProvider<TestValue> Storage => (IStorageProvider<TestValue>)
        Provider!.GetRequiredService<INamedOptions<StorageOptions>>().Value.Providers[typeof(TestValue)].Create(Provider!);
}
