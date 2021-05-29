using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Client.Tests.Mocks;

namespace Assistant.Net.Messaging.Web.Client.Tests
{
    public class RemoteCommandHandlingClientTests
    {
        private const string RequestUri = "http://localhost";
        private static readonly TestScenarioCommand ValidCommand = new(0);
        private static readonly TestResponse SuccessResponse = new(true);
        private static readonly TestResponse InvalidlyParsedResponse = new(false);
        private static readonly CommandFailedException FailedResponse = new("test");

        [Test]
        public async Task DelegateHandling_sendsHttpRequestMessage()
        {
            var handler = new TestDelegatingHandler(SuccessResponse);
            var client = Client(handler);

            await client.DelegateHandling(ValidCommand);

            handler.Request.Should().BeEquivalentTo(new HttpRequestMessage(HttpMethod.Post, RequestUri)
            {
                Headers = { { HeaderNames.CommandName, nameof(TestScenarioCommand) } },
                Content = JsonContent.Create(new TestScenarioCommand(0))
            });
        }

        [Test]
        public async Task DelegateHandling_returnsTestResponse()
        {
            var handler = new TestDelegatingHandler(SuccessResponse);
            var client = Client(handler);

            var response = await client.DelegateHandling(ValidCommand);

            response.Should().Be(SuccessResponse);
        }

        [Test]
        public async Task DelegateHandling_returnsCommandFailedException()
        {
            var handler = new TestDelegatingHandler(FailedResponse);
            var client = Client(handler);

            var response = await client.DelegateHandling(ValidCommand);

            response.Should().Be(InvalidlyParsedResponse);
        }

        private static RemoteCommandHandlingClient Client(DelegatingHandler handler)
        {
            var services = new ServiceCollection();
            services
                .AddSystemLifetime()
                .AddTypeEncoder()
                .AddJsonSerializerOptions()
                .AddHttpClient<RemoteCommandHandlingClient>(c => c.BaseAddress = new Uri(RequestUri))
                .AddHttpMessageHandler(() => handler);
            return services
                .BuildServiceProvider()
                .GetRequiredService<RemoteCommandHandlingClient>();
        }
    }
}