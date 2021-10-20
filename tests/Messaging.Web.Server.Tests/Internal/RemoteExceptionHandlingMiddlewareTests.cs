using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Server.Tests.Fixtures;
using Assistant.Net.Messaging.Web.Server.Tests.Mocks;
using Assistant.Net.Serialization.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Server.Tests.Internal
{
    [Timeout(2000)]
    public class RemoteExceptionHandlingMiddlewareTests
    {
        private static readonly string CorrelationId = Guid.NewGuid().ToString();

        [TestCase(typeof(TimeoutException))]
        [TestCase(typeof(TaskCanceledException))]
        [TestCase(typeof(OperationCanceledException))]
        [TestCase(typeof(MessageDeferredException))]
        public async Task Post_Accepted_thrownInterruptingKindOfException(Type exceptionType)
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestFailMessageHandler>()
                .Create();

            var request = await Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Accepted,
                RequestMessage = request,
                Headers =
                {
                    {HeaderNames.MessageName, nameof(TestFailMessage)},
                    {HeaderNames.CorrelationId, CorrelationId},
                }
            });
        }

        [Test]
        public async Task Post_NotFound_thrownMessageNotFoundException()
        {
            var exceptionType = typeof(MessageNotFoundException);
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestFailMessageHandler>()
                .Create();

            var request = await Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NotFound,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.MessageName, Value = new[] { nameof(TestFailMessage) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<MessageException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<MessageNotFoundException>();
        }

        [Test]
        public async Task Post_NotFound_thrownMessageNotRegisteredException()
        {
            var exceptionType = typeof(MessageNotRegisteredException);
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestFailMessageHandler>()
                .Create();

            var request = await Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NotFound,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.MessageName, Value = new[] { nameof(TestFailMessage) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<MessageException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<MessageNotRegisteredException>();
        }

        [Test]
        public async Task Post_BadRequest_thrownMessageContractException()
        {
            var exceptionType = typeof(MessageContractException);
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestFailMessageHandler>()
                .Create();

            var request = await Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.BadRequest,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.MessageName, Value = new[] { nameof(TestFailMessage) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<MessageException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<MessageContractException>();
        }

        [Test]
        public async Task Post_InternalServerError_throwAnyOtherMessageException()
        {
            var exceptionType = typeof(TestMessageException);
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestFailMessageHandler>()
                .Create();

            var request = await Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.InternalServerError,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.MessageName, Value = new[] { nameof(TestFailMessage) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<MessageException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<TestMessageException>();
        }

        private static async Task<HttpRequestMessage> Request<T>(T message) where T : IAbstractMessage =>
            new(HttpMethod.Post, "http://localhost/messages")
            {
                Headers =
                {
                    {HeaderNames.MessageName, message.GetType().Name},
                    {HeaderNames.CorrelationId, CorrelationId}
                },
                Content = new ByteArrayContent(await Binary(message))
            };

        private static Task<byte[]> Binary<T>(T value) => Provider.GetRequiredService<ISerializer<T>>().Serialize(value);

        private static readonly IServiceProvider Provider = new ServiceCollection().AddJsonSerialization().BuildServiceProvider();
    }
}