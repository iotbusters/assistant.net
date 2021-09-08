using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading;
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
        public async Task Intercept_returnsResponse()
        {
            var response = await interceptor.Intercept((_,_) => Task.FromResult<object>(new TestResponse(false)), Message);

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task Intercept_throwsMessageExecutionException()
        {
            await interceptor.Awaiting(x => x.Intercept(Fail(new TestMessageExecutionException()), Message))
                .Should().ThrowAsync<TestMessageExecutionException>();
        }

        [Test]
        public async Task Intercept_throwsOperationCanceledException()
        {
            await interceptor.Awaiting(x => x.Intercept(Fail(new OperationCanceledException()), Message))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task Intercept_throwsTaskCanceledException()
        {
            await interceptor.Awaiting(x => x.Intercept(Fail(new TaskCanceledException()), Message))
                .Should().ThrowAsync<TaskCanceledException>();
        }

        [Test]
        public async Task Intercept_throwsTimeoutException()
        {
            await interceptor.Awaiting(x => x.Intercept(Fail(new TimeoutException()), Message))
                .Should().ThrowAsync<TimeoutException>();
        }

        [Test]
        public async Task Intercept_throwsException()
        {
            await interceptor.Awaiting(x => x.Intercept(Fail(new Exception()), Message))
                .Should().ThrowAsync<Exception>();
        }

        private static Func<IMessage<object>, CancellationToken, Task<object>> Fail(Exception ex) => (_, _) => throw ex;
    }
}