using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Client.Tests.Mocks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Extensions;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Messaging.Web.Client.Tests
{
    public class RemoteCommandHandlingClientTests
    {
        private const string RequestUri = "http://localhost";
        private static readonly TestScenarioCommand ValidCommand = new(0);
        private static readonly TestResponse SuccessResponse = new(true);
        private static readonly CommandFailedException FailedResponse = new("test");

        private static Task<byte[]> Binary<T>(T value) => Provider.GetRequiredService<ISerializer<T>>().Serialize(value);
        private static readonly IServiceProvider Provider = new ServiceCollection().AddJsonSerialization().BuildServiceProvider();

        [Test]
        public async Task DelegateHandling_sendsHttpRequestMessage()
        {
            var handler = new TestDelegatingHandler(await Binary(SuccessResponse), HttpStatusCode.OK);
            var client = Client(handler);

            await client.DelegateHandling(ValidCommand);

            handler.Request.Should().BeEquivalentTo(new HttpRequestMessage(HttpMethod.Post, RequestUri)
            {
                Headers = { { HeaderNames.CommandName, nameof(TestScenarioCommand) } },
                Content = new ByteArrayContent(await Binary(SuccessResponse))
            });
        }

        [Test]
        public async Task DelegateHandling_returnsTestResponse()
        {
            var handler = new TestDelegatingHandler(await Binary(SuccessResponse), HttpStatusCode.OK);
            var client = Client(handler);

            var response = await client.DelegateHandling(ValidCommand);

            response.Should().Be(SuccessResponse);
        }

        [Test]
        public async Task DelegateHandling_throwCommandFailedException()
        {
            var handler = new TestDelegatingHandler(await Binary(FailedResponse), HttpStatusCode.InternalServerError);
            var client = Client(handler);

           await client.Awaiting(x => x.DelegateHandling(ValidCommand))
               .Should().ThrowAsync<CommandFailedException>()
               .WithMessage("test");
        }

        private static IRemoteCommandClient Client(DelegatingHandler handler)
        {
            var services = new ServiceCollection();
            services
                .AddRemoteWebCommandClient(c => c.BaseAddress = new Uri(RequestUri))
                .ClearAllHttpMessageHandlers()
                .AddHttpMessageHandler<ErrorPropagationHandler>()
                .AddHttpMessageHandler(() => handler);
            return services
                .BuildServiceProvider()
                .GetRequiredService<IRemoteCommandClient>();
        }
    }
}