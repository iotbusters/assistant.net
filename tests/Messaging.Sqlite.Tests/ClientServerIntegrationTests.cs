using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Sqlite.Tests.Fixtures;
using Assistant.Net.Messaging.Sqlite.Tests.Mocks;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Sqlite.Tests;

[Timeout(2000)]
public class ClientServerIntegrationTests
{
    [TestCase(5)]
    public async Task RequestObject_onceCallsHandler_concurrently(int concurrencyCount)
    {
        var handler = new TestScenarioMessageHandler();
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler(handler)
            .Create();

        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            _ => fixture.Client.RequestObject(new TestScenarioMessage(0))).ToArray();
        await Task.WhenAll(tasks);

        handler.CallCount.Should().Be(1);
    }

    [TestCase(5)]
    public async Task RequestObject_neverCallsHandler_concurrently(int concurrencyCount)
    {
        var handler = new TestScenarioMessageHandler();
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler(handler)
            .Create();
        await fixture.Client.RequestObject(new TestScenarioMessage(0));
        handler.CallCount = 0;

        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            _ => fixture.Client.RequestObject(new TestScenarioMessage(0))).ToArray();
        await Task.WhenAll(tasks);

        handler.CallCount.Should().Be(0);
    }

    [Test]
    public async Task RequestObject_returnsResponse_singleProvider()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteSingleProvider(ConnectionString)
            .AddSingleProviderHandler<TestScenarioMessageHandler>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsResponse()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsAnotherResponse_serverSideHandlerChanged()
    {
        // global arrange
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
            .Create();

        // arrange 1
        fixture.ReplaceHandlers(new TestMessageHandler<TestScenarioMessage, TestResponse>(new TestResponse(true)));

        // act 1
        var response1 = await fixture.Client.Request(new TestScenarioMessage(1));

        // assert 1
        response1.Should().BeEquivalentTo(new TestResponse(true));

        // arrange 2
        fixture.ReplaceHandlers(new TestMessageHandler<TestScenarioMessage, TestResponse>(new TestResponse(false)));

        // act 2
        var response2 = await fixture.Client.Request(new TestScenarioMessage(2));

        // assert 2
        response2.Should().BeEquivalentTo(new TestResponse(false));
    }

    [Test]
    public void RequestObject_throwsMessageNotRegisteredException_NoLocalHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestSuccessFailureMessageHandler>()
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
            .Result.Which.InnerException.Should().BeNull();
    }

    [Test, Ignore("No way to check remote handlers.")]
    public void RequestObject_throwsMessageNotRegisteredException_NoRemoteHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddMessageRegistrationOnly<TestScenarioMessage>()
            .AddHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
            .Result.Which.InnerException.Should().BeNull();
    }

    [Test]
    public void RequestObject_throwsTimeoutException_thrownTimeoutException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestSuccessFailureMessageHandler>()
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestSuccessFailureMessage(typeof(TimeoutException).AssemblyQualifiedName)))
            .Should().ThrowExactlyAsync<TimeoutException>()
            .Result.Which.InnerException.Should().BeNull();
    }

    [Test]
    public void RequestObject_throwsMessageDeferredException_thrownMessageDeferredException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestSuccessFailureMessageHandler>()
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestSuccessFailureMessage(typeof(MessageDeferredException).AssemblyQualifiedName)))
            .Should().ThrowExactlyAsync<MessageDeferredException>()
            .Result.Which.InnerException.Should().BeNull();
    }

    [Test]
    public void RequestObject_throwsMessageFailedException_thrownInvalidOperationException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(1)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("Message handling has failed.")
            .Result.Which.InnerException.Should().BeNull();
    }

    [Test]
    public void RequestObject_throwsMessageFailedException_thrownMessageFailedException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(2)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("2")
            .Result.Which.InnerException.Should().BeNull();
    }

    [Test]
    public void RequestObject_throwsMessageFailedException_thrownMessageFailedExceptionWithInnerException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqlite(ConnectionString)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(3)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("3")
            .Result.WithInnerExceptionExactly<MessageFailedException>()
            .Which.InnerException?.Message.Should().Be("inner");
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await MasterConnection.OpenAsync(CancellationToken);
        Provider = new ServiceCollection()
            .AddStorage(b => b.UseSqlite(MasterConnection))
            .BuildServiceProvider();
        var dbContext = await Provider.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync(CancellationToken);
        await dbContext.Database.EnsureCreatedAsync(CancellationToken);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Provider?.Dispose();
        MasterConnection.Dispose();
    }

    [TearDown]
    public async Task TearDown()
    {
        var dbContext = await Provider!.GetRequiredService<IDbContextFactory<StorageDbContext>>().CreateDbContextAsync();
        dbContext.HistoricalKeys.RemoveRange(dbContext.HistoricalKeys);
        dbContext.StorageKeys.RemoveRange(dbContext.StorageKeys);
        await dbContext.SaveChangesAsync();
    }

    private const string ConnectionString = "Data Source=test;Mode=Memory;Cache=Shared";
    /// <summary>
    ///     Shared SQLite in-memory database connection keeping the data shared between other connections.
    /// </summary>
    private SqliteConnection MasterConnection { get; } = new(ConnectionString);
    private static CancellationToken CancellationToken => new CancellationTokenSource(5000).Token;
    private ServiceProvider? Provider { get; set; }

}
