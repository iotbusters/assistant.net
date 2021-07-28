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
        [TestCase(typeof(CommandDeferredException))]
        public async Task Post_Accepted_thrownInterruptingKindOfException(Type exceptionType)
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestFailCommandHandler>()
                .Create();

            var request = await Request(new TestFailCommand(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Accepted,
                RequestMessage = request,
                Headers =
                {
                    {HeaderNames.CommandName, nameof(TestFailCommand)},
                    {HeaderNames.CorrelationId, CorrelationId},
                }
            });
        }

        [Test]
        public async Task Post_NotFound_thrownCommandNotFoundException()
        {
            var exceptionType = typeof(CommandNotFoundException);
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestFailCommandHandler>()
                .Create();

            var request = await Request(new TestFailCommand(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NotFound,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.CommandName, Value = new[] { nameof(TestFailCommand) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<CommandException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<CommandNotFoundException>();
        }

        [Test]
        public async Task Post_NotFound_thrownCommandNotRegisteredException()
        {
            var exceptionType = typeof(CommandNotRegisteredException);
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestFailCommandHandler>()
                .Create();

            var request = await Request(new TestFailCommand(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.NotFound,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.CommandName, Value = new[] { nameof(TestFailCommand) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<CommandException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<CommandNotRegisteredException>();
        }

        [Test]
        public async Task Post_BadRequest_thrownCommandContractException()
        {
            var exceptionType = typeof(CommandContractException);
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestFailCommandHandler>()
                .Create();

            var request = await Request(new TestFailCommand(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.BadRequest,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.CommandName, Value = new[] { nameof(TestFailCommand) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<CommandException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<CommandContractException>();
        }

        [Test]
        public async Task Post_InternalServerError_throwAnyOtherCommandException()
        {
            var exceptionType = typeof(TestCommandException);
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestFailCommandHandler>()
                .Create();

            var request = await Request(new TestFailCommand(exceptionType.AssemblyQualifiedName));
            var response = await fixture.Client.SendAsync(request);

            response.Should().BeEquivalentTo(new
            {
                StatusCode = HttpStatusCode.InternalServerError,
                RequestMessage = request,
                Headers = new[]
                {
                    new { Key = HeaderNames.CommandName, Value = new[] { nameof(TestFailCommand) } },
                    new { Key = HeaderNames.CorrelationId, Value = new[] { CorrelationId } }
                }
            });
            var responseObject = await response.Content.ReadFromJsonAsync<CommandException>(fixture.JsonSerializerOptions);
            responseObject.Should().BeOfType<TestCommandException>();
        }

        private static async Task<HttpRequestMessage> Request<T>(T command) where T : IAbstractCommand =>
            new(HttpMethod.Post, "http://localhost/command")
            {
                Headers =
                {
                    {HeaderNames.CommandName, command.GetType().Name},
                    {HeaderNames.CorrelationId, CorrelationId}
                },
                Content = new ByteArrayContent(await Binary(command))
            };

        private static Task<byte[]> Binary<T>(T value) => Provider.GetRequiredService<ISerializer<T>>().Serialize(value);

        private static readonly IServiceProvider Provider = new ServiceCollection().AddJsonSerialization().BuildServiceProvider();
    }
}