using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Interceptors
{
    public class CachingInterceptorTests
    {
        private IMessageInterceptor interceptor = null!;
        private IStorage<string, CachingResult> cache = null!;
        private static readonly TestMessage Message = new(0);

        [SetUp]
        public void Setup()
        {
            var provider = new ServiceCollection()
                .AddSystemClock()
                .AddStorage(b => b.AddLocal<string, CachingResult>())
                .AddTransient<CachingInterceptor>()
                .BuildServiceProvider();
            interceptor = provider.GetRequiredService<CachingInterceptor>();
            cache = provider.GetRequiredService<IStorage<string, CachingResult>>();
        }

        [Test]
        public async Task Intercept_returnsResponse()
        {
            var response = await interceptor.Intercept((_, _) => Task.FromResult<object>(new TestResponse(false)), Message);

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task Intercept_returnsResponseFromCache()
        {
            await interceptor.Intercept((_, _) => Task.FromResult<object>(new TestResponse(false)), Message);

            var response = await interceptor.Intercept((_, _) => Task.FromResult<object>(new TestResponse(true)), Message);

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task Intercept_throwsMessageExecutionException()
        {
            await interceptor.Awaiting(x => x.Intercept(Fail(new TestMessageExecutionException()), Message))
                .Should().ThrowAsync<TestMessageExecutionException>();
        }

        [Test]
        public async Task Intercept_throwsMessageExecutionExceptionFromCache()
        {
            await cache.AddOrGet(Message.GetSha1(), _ => new CachingExceptionResult(new TestMessageExecutionException()));

            await interceptor.Awaiting(x => x.Intercept(Fail(new ArgumentException()), Message))
                .Should().ThrowAsync<TestMessageExecutionException>();
        }

        private static Func<IMessage<object>, CancellationToken, Task<object>> Fail(Exception ex) => (_, _) => throw ex;
    }
}
