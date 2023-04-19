using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Mongo.Tests.Fixtures;
using Assistant.Net.Messaging.Mongo.Tests.Mocks;
using Assistant.Net.Storage;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Mongo.Tests;

[Timeout(2000)]
public class ClientServerIntegrationTests
{
    [TestCase(5)]
    public async Task RequestObject_calls5TimesHandler_concurrently(int concurrencyCount)
    {
        var handler = new TestScenarioMessageHandler();
        using var fixture = new MessagingClientFixtureBuilder()
            .UseMongo(ConnectionString, Database)
            .AddHandler(handler)
            .Create();

        var tasks = Enumerable.Range(1, concurrencyCount).Select(
            _ => fixture.Client.RequestObject(new TestScenarioMessage(0))).ToArray();
        await Task.WhenAll(tasks);

        handler.CallCount.Should().Be(5);
    }

    [Test]
    public async Task RequestObject_returnsResponse()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseMongo(ConnectionString, Database)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsResponse_usingBackoffHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseMongo(ConnectionString, Database)
            .UseRemoteBackoffHandler<TestScenarioMessageHandler>()
            .AddMessageRegistrationOnly<TestScenarioMessage>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_returnsAnotherResponse_serverSideHandlerChanged()
    {
        // global arrange
        using var fixture = new MessagingClientFixtureBuilder()
            .UseMongo(ConnectionString, Database)
            .AddHandler<TestSuccessFailureMessageHandler>() // to have at least one handler configured
            .Create();

        // arrange 1
        fixture.ReplaceHandlers(new TestMessageHandler<TestScenarioMessage, TestResponse>(new(true)));

        // act 1
        var response1 = await fixture.Client.Request(new TestScenarioMessage(1));

        // assert 1
        response1.Should().BeEquivalentTo(new TestResponse(true));

        // arrange 2
        fixture.ReplaceHandlers(new TestMessageHandler<TestScenarioMessage, TestResponse>(new(false)));

        // act 2
        var response2 = await fixture.Client.Request(new TestScenarioMessage(2));

        // assert 2
        response2.Should().BeEquivalentTo(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_noLocalHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseMongo(ConnectionString, Database)
            .AddHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
            .Create();

        await fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{typeof(TestScenarioMessage)}' wasn't registered.");
    }

    [Test, Ignore("No way to check remote handlers.")]
    public async Task RequestObject_throwsMessageNotRegisteredException_noRemoteHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseMongo(ConnectionString, Database)
            .AddMessageRegistrationOnly<TestScenarioMessage>()
            .AddHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
            .Create();

        await fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{typeof(TestScenarioMessage)}' wasn't registered.");
    }

    [Test]
    public void RequestObject_throwsTimeoutException_thrownTimeoutException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .UseMongo(ConnectionString, Database)
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
            .UseMongo(ConnectionString, Database)
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
            .UseMongo(ConnectionString, Database)
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
            .UseMongo(ConnectionString, Database)
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
            .UseMongo(ConnectionString, Database)
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
            .UseMongo(ConnectionString, Database)
            .AddHandler<TestScenarioMessageHandler>()
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
            .UseMongo(ConnectionString, Database)
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        // assert 1
        await fixture.Client.RequestObject(message);

        // arrange 2
        await fixture.GetServerService<ServerAvailabilityService>().Unregister(string.Empty, default);

        // assert 2
        await fixture.Awaiting(x => x.Client.RequestObject(message))
            .Should().ThrowAsync<MessageNotRegisteredException>();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        Provider = new ServiceCollection()
            .ConfigureMongoOptions(o => o.Connection(ConnectionString).Database("test"))
            .AddMongoClient()
            .BuildServiceProvider();

        string pingContent;
        var mongoClient = Provider.GetRequiredService<IMongoClient>();
        try
        {
            var ping = await mongoClient.GetDatabase("db").RunCommandAsync(
                (Command<BsonDocument>)"{ping:1}",
                ReadPreference.Nearest,
                new CancellationTokenSource(500).Token);
            pingContent = ping.ToString();
        }
        catch
        {
            pingContent = string.Empty;
        }
        if (!pingContent.Contains("ok"))
            Assert.Ignore($"The tests require mongodb instance at {ConnectionString}.");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider?.Dispose();
        
    [SetUp, TearDown]
    public async Task Cleanup()
    {
        var mongoClient = Provider!.GetRequiredService<IMongoClient>();
        await mongoClient.DropDatabaseAsync(Database);
    }

    private const string ConnectionString = "mongodb://127.0.0.1:27017";
    private const string Database = "test";
    public ServiceProvider? Provider { get; set; }
}
