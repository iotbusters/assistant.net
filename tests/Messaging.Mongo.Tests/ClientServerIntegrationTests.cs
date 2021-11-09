using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Mongo.Tests.Fixtures;
using Assistant.Net.Messaging.Mongo.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Mongo.Tests
{
    [Timeout(2000)]
    public class ClientServerIntegrationTests
    {
        [TestCase(5)]
        public async Task Send_onceCallsHandler_concurrently(int concurrencyCount)
        {
            var handler = new TestScenarioMessageHandler();
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler(handler)
                .Create();

            var tasks = Enumerable.Range(1, concurrencyCount).Select(
                _ => fixture.Client.RequestObject(new TestScenarioMessage(0)));
            await Task.WhenAll(tasks);

            handler.CallCount.Should().Be(1);
        }

        [TestCase(5)]
        public async Task Send_neverCallsHandler_concurrently(int concurrencyCount)
        {
            var handler = new TestScenarioMessageHandler();
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler(handler)
                .Create();
            await fixture.Client.RequestObject(new TestScenarioMessage(0));
            handler.CallCount = 0;

            var tasks = Enumerable.Range(1, concurrencyCount).Select(
                _ => fixture.Client.RequestObject(new TestScenarioMessage(0)));
            await Task.WhenAll(tasks);

            handler.CallCount.Should().Be(0);
        }

        [Test]
        public async Task Send_returnsResponse()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler<TestScenarioMessageHandler>()
                .Create();

            var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

            response.Should().Be(new TestResponse(false));
        }

        [Test]
        public void Send_throwsMessageNotRegisteredException_NoLocalHandler()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler<TestSuccessFailureMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [Test, Ignore("No way to check remote handlers.")]
        public void Send_throwsMessageNotRegisteredException_NoRemoteHandler()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoMessageRegistrationOnly<TestScenarioMessage>()
                .AddMongoHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [TestCase(typeof(TimeoutException))]
        [TestCase(typeof(MessageDeferredException))]
        public void Send_throwsInterruptingKindOfException_thrownMessageDeferredException(Type exceptionType)
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler<TestSuccessFailureMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestSuccessFailureMessage(exceptionType.AssemblyQualifiedName)))
                .Should().ThrowExactly<MessageDeferredException>()
                .WithMessage("No response from server in defined amount of time.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsMessageFailedException_thrownInvalidOperationException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(1)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("Message handling has failed.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsMessageFailedException_thrownMessageFailedException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(2)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("2")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsMessageFailedException_thrownMessageFailedExceptionWithInnerException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .UseMongo(ConnectionString, Database)
                .AddMongoHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(3)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("3")
                .WithInnerExceptionExactly<MessageFailedException>()
                .Which.InnerException?.Message.Should().Be("inner");
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Provider = new ServiceCollection()
                .ConfigureMongoOptions(ConnectionString)
                .AddMongoClientFactory()
                .BuildServiceProvider();

            string pingContent;
            var mongoClient = Provider.GetRequiredService<IMongoClientFactory>().CreateClient();
            try
            {
                var ping = await mongoClient.GetDatabase("db").RunCommandAsync(
                    (Command<BsonDocument>)"{ping:1}",
                    ReadPreference.Nearest,
                    new CancellationTokenSource(200).Token);
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
            var mongoClient = Provider!.GetRequiredService<IMongoClientFactory>().CreateClient();
            await mongoClient.DropDatabaseAsync(Database);
        }

        private const string ConnectionString = "mongodb://127.0.0.1:27017";
        private const string Database = "test";
        public ServiceProvider? Provider { get; set; }
    }
}
