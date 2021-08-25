using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Interceptors
{
    public class ErrorHandlingInterceptorTests
    {
        private IMessageInterceptor<IMessage<object>, object> interceptor = null!;
        private static readonly TestMessage Message = new(0);

        [SetUp]
        public void Setup()
        {
            interceptor = new ServiceCollection()
                .AddTransient<ErrorHandlingInterceptor>()
                .BuildServiceProvider()
                .GetRequiredService<ErrorHandlingInterceptor>();
        }

        [Test]
        public async Task ReturnsResponse()
        {
            var response = await interceptor.Intercept(Message, _ => Task.FromResult<object>(new TestResponse(false)));

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task ThrowsMessageExecutionException()
        {
            await interceptor.Awaiting(x => x.Intercept(Message, Fail(new TestMessageExecutionException())))
                .Should().ThrowAsync<TestMessageExecutionException>();
        }

        [Test]
        public async Task ThrowsOperationCanceledException()
        {
            await interceptor.Awaiting(x => x.Intercept(Message, Fail(new OperationCanceledException())))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task ThrowsTaskCanceledException()
        {
            await interceptor.Awaiting(x => x.Intercept(Message, Fail(new TaskCanceledException())))
                .Should().ThrowAsync<TaskCanceledException>();
        }

        [Test]
        public async Task ThrowsTimeoutException()
        {
            await interceptor.Awaiting(x => x.Intercept(Message, Fail(new TimeoutException())))
                .Should().ThrowAsync<TimeoutException>();
        }

        [Test]
        public async Task ThrowsException()
        {
            await interceptor.Awaiting(x => x.Intercept(Message, Fail(new Exception())))
                .Should().ThrowAsync<Exception>();
        }

        private static Func<IMessage<object>, Task<object>> Fail(Exception ex) => _ => throw ex;
    }
}