using Assistant.Net.Diagnostics;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Sqlite.Tests.Mocks;
using Assistant.Net.Unions;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Sqlite.Tests.Internal;

public class SqliteStorageProviderTestsIntegrationTests
{
    [Test]
    public async Task AddOrGet_returnsAddedValue_notExists()
    {
        var value = await Storage.AddOrGet(TestKey, TestValue("added"));

        value.Should().BeEquivalentTo(new {Type = "added"});
    }

    [Test]
    public async Task AddOrGet_returnsExistingValue_exists()
    {
        await Storage.AddOrGet(TestKey, TestValue("added-1"));

        var value = await Storage.AddOrGet(TestKey, TestValue("added-2"));

        value.Should().BeEquivalentTo(
            TestValue("added-1") with {Audit = Audit()},
            o => o.ComparingByMembers<ValueRecord>());
    }

    [TestCase(5)]
    public async Task AddOrGet_returnsValuesAndInitialVersion_concurrently(int concurrencyCount)
    {
        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            i => Storage.AddOrGet(TestKey, TestValue($"value-{i}")));
        var values = await Task.WhenAll(tasks);

        var lastValue = await Storage.TryGet(TestKey);

        lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit()}});
        values.Select(x => x.Type).Distinct().Should().HaveCount(1);
    }

    [Test]
    public async Task AddOrUpdate_returnsAddedValue()
    {
        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added"), (_, _) => TestValue("updated"));

        value.Should().BeEquivalentTo(
            TestValue("added") with {Audit = Audit()},
            o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task AddOrUpdate_returnsUpdatedValue()
    {
        await Storage.AddOrGet(TestKey, TestValue("added-1", version: 1));

        var value = await Storage.AddOrUpdate(TestKey, _ => TestValue("added-2", version: 2), (_, _) => TestValue("updated", version: 3));

        value.Should().BeEquivalentTo(TestValue("updated", version: 3), o => o.ComparingByMembers<ValueRecord>());
    }

    [TestCase(5)]
    public async Task AddOrUpdate_returnsValuesAndLastVersion_concurrently(int concurrencyCount)
    {
        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            i => Storage.AddOrUpdate(TestKey, TestValue($"value-{i}", version: i)));
        var values = await Task.WhenAll(tasks);

        var lastValue = await Storage.TryGet(TestKey);

        lastValue.Should().BeEquivalentTo(new {Value = new {Audit = Audit(concurrencyCount)}});
        values.Should().BeEquivalentTo(Enumerable.Range(1, concurrencyCount).Select(i => new {Type = $"value-{i}"}));
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
        await Storage.AddOrGet(TestKey, TestValue("value"));

        var value = await Storage.TryGet(TestKey);

        value.Should().BeEquivalentTo(
            new {Value = TestValue("value") with {Audit = Audit()}},
            o => o.ComparingByMembers<ValueRecord>());
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
        await Storage.AddOrGet(TestKey, TestValue("value"));

        var value = await Storage.TryRemove(TestKey);

        value.Should().BeEquivalentTo(
            new {Value = TestValue("value") with {Audit = Audit()}},
            o => o.ComparingByMembers<ValueRecord>());
    }

    [Test]
    public async Task GetKeys_returnsKeys()
    {
        await Storage.AddOrGet(TestKey, TestValue("value"));

        var value = Storage.GetKeys().ToArray();

        value.Should().BeEquivalentTo(new[] {TestKey});
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await MasterConnection.OpenAsync(CancellationToken);
        Provider = new ServiceCollection()
            .AddStorage(b => b
                .UseSqlite(ConnectionString)
                .AddSqlite<TestKey, TestValue>())
            .AddDiagnosticContext(getCorrelationId: _ => TestCorrelationId, getUser: _ => TestUser)
            .AddSystemClock(_ => TestDate)
            .BuildServiceProvider();
        var dbContext = await Provider.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync(CancellationToken);
        await dbContext.Database.EnsureCreatedAsync(CancellationToken);
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

    private const string ConnectionString = "Data Source=test;Mode=Memory;Cache=Shared";
    /// <summary>
    ///     Shared SQLite in-memory database connection keeping the data shared between other connections.
    /// </summary>
    private SqliteConnection MasterConnection { get; } = new(ConnectionString);
    private static CancellationToken CancellationToken => new CancellationTokenSource(500).Token;
    private ValueRecord TestValue(string type, int version = 1) => new(Type: type, Content: Array.Empty<byte>(), Audit(version));
    private Audit Audit(int version = 1) => new(TestCorrelationId, TestUser, TestDate, version);
    private KeyRecord TestKey { get; } = new(id: $"test-{Guid.NewGuid()}", type: "test-key", content: Array.Empty<byte>());
    private string TestCorrelationId { get; set; } = Guid.NewGuid().ToString();
    private string TestUser { get; set; } = Guid.NewGuid().ToString();
    private DateTimeOffset TestDate { get; } = DateTimeOffset.UtcNow;
    private ServiceProvider? Provider { get; set; }
    private StorageDbContext? DbContext { get; set; }
    private IStorageProvider<TestValue> Storage => Provider!.CreateScope().ServiceProvider.GetRequiredService<IStorageProvider<TestValue>>();
}
