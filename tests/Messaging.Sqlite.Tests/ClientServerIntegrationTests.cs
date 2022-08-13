using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Sqlite.Tests.Fixtures;
using Assistant.Net.Messaging.Sqlite.Tests.Mocks;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Sqlite.Tests;

[Timeout(3000)]
public class ClientServerIntegrationTests
{
    [TestCase(5)]
    public async Task RequestObject_calls5TimesHandler_concurrently(int concurrencyCount)
    {
        var handler = new TestScenarioMessageHandler();
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteProvider(ConnectionString)
            .AddHandler(handler)
            .Create();

        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            _ => fixture.Client.RequestObject(new TestScenarioMessage(0))).ToArray();
        await Task.WhenAll(tasks);

        handler.CallCount.Should().Be(5);
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
    public async Task RequestObject_returnsResponse_AnySingleProvider()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteSingleProvider(ConnectionString)
            .AddAnySingleProviderHandler<TestScenarioMessageHandler>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsResponse_AnyProvider()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteSingleProvider(ConnectionString)
            .AddAnyProviderHandler<TestScenarioMessageHandler>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsResponse()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteProvider(ConnectionString)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsResponse_nestedRequestCall()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteProvider(ConnectionString)
            .AddHandler<TestNestedMessageHandler>()
            .AddHandler(new TestMessageHandler<TestRequest, TestResponse>(new TestResponse(false)))
            .Create();

        var response = await fixture.Client.RequestObject(new TestNestedRequest());

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsNothing_nestedPublishCall()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteProvider(ConnectionString)
            .AddHandler<TestNestedMessageHandler>()
            .AddHandler(new TestEventHandler<TestEvent>())
            .Create();

        await fixture.Client.RequestObject(new TestNestedEvent());
    }

    [Test]
    public async Task RequestObject_returnsAnotherResponse_serverSideHandlerChanged()
    {
        // global arrange
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteProvider(ConnectionString)
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
    public void RequestObject_throwsMessageNotRegisteredException_noLocalHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteProvider(ConnectionString)
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
            .UseSqliteProvider(ConnectionString)
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
            .UseSqliteProvider(ConnectionString)
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
            .UseSqliteProvider(ConnectionString)
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
            .UseSqliteProvider(ConnectionString)
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
            .UseSqliteProvider(ConnectionString)
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
            .UseSqliteProvider(ConnectionString)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(3)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("3")
            .Result.WithInnerExceptionExactly<MessageFailedException>()
            .Which.InnerException?.Message.Should().Be("inner");
    }

    [Test]
    public async Task RequestObject_throwsMessageDeferredException_inactiveServer()
    {
        // global arrange
        var message = new TestScenarioMessage(0);
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteSingleProvider(ConnectionString)
            .AddSingleProviderHandler<TestScenarioMessageHandler>()
            .Create();

        // assert 1
        await fixture.Client.RequestObject(message);

        // arrange 2
        var activityService = fixture.GetServerService<ServerActivityService>();
        activityService.Inactivate();

        // assert 2
        await fixture.Awaiting(x => x.Client.RequestObject(message))
            .Should().ThrowAsync<MessageDeferredException>();
    }

    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_unavailableServer()
    {
        // global arrange
        var message = new TestScenarioMessage(0);
        using var fixture = new MessagingClientFixtureBuilder()
            .UseSqliteSingleProvider(ConnectionString)
            .AddSingleProviderHandler<TestScenarioMessageHandler>()
            .Create();

        // assert 1
        await fixture.Client.RequestObject(message);

        // arrange 2
        await fixture.GetServerService<ServerAvailabilityService>().Unregister(default);

        // assert 2
        await fixture.Awaiting(x => x.Client.RequestObject(message))
            .Should().ThrowAsync<MessageNotRegisteredException>();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await MasterConnection.OpenAsync(CancellationToken);
        Provider = new ServiceCollection()
            .AddStorage(b => b.UseSqlite(ConnectionString))
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

    /// <summary>
    ///     Shared SQLite in-memory database connection string (see <see cref="MasterConnection"/>).
    /// </summary>
    private const string ConnectionString = "Data Source=test;Mode=Memory;Cache=Shared";
    /// <summary>
    ///     Shared SQLite in-memory database connection keeping the data shared between other connections.
    /// </summary>
    private SqliteConnection MasterConnection { get; } = new(ConnectionString);
    private static CancellationToken CancellationToken => new CancellationTokenSource(5000).Token;
    private ServiceProvider? Provider { get; set; }

}
