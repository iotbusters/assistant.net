using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Messaging.Interceptors;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Assistant.Net.Messaging.Tests.Interceptors
{
    public class ErrorHandlingInterceptorTests
    {
        private ICommandInterceptor<ICommand<object>, object> interceptor = null!;
        private static readonly TestCommand Command = new(0);

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
            var response = await interceptor.Intercept(Command, _ => Task.FromResult<object>(new TestResponse(false)));

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task ThrowsCommandExecutionException()
        {
            await interceptor.Awaiting(x => x.Intercept(Command, Fail(new TestCommandExecutionException())))
                .Should().ThrowAsync<TestCommandExecutionException>();
        }

        [Test]
        public async Task ThrowsOperationCanceledException()
        {
            await interceptor.Awaiting(x => x.Intercept(Command, Fail(new OperationCanceledException())))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task ThrowsTaskCanceledException()
        {
            await interceptor.Awaiting(x => x.Intercept(Command, Fail(new TaskCanceledException())))
                .Should().ThrowAsync<TaskCanceledException>();
        }

        [Test]
        public async Task ThrowsTimeoutException()
        {
            await interceptor.Awaiting(x => x.Intercept(Command, Fail(new TimeoutException())))
                .Should().ThrowAsync<TimeoutException>();
        }

        [Test]
        public async Task ThrowsException()
        {
            await interceptor.Awaiting(x => x.Intercept(Command, Fail(new Exception())))
                .Should().ThrowAsync<Exception>();
        }

        private static Func<ICommand<object>, Task<object>> Fail(Exception ex) => _ => throw ex;
    }
}