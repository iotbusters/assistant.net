using Assistant.Net.Abstractions;
using Assistant.Net.Dynamics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Assistant.Net.Core.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void Decorate_interceptsOriginalCall()
        {
            var t1 = new DateTimeOffset(new DateTime(2001, 1, 1));
            var t2 = new DateTimeOffset(new DateTime(2002, 2, 2));
            var decoratedClock = new ServiceCollection()
                .AddSystemClock(_ => t1)
                .Decorate<ISystemClock>(proxy => proxy.Intercept(x => x.UtcNow, _ => t2))
                .BuildServiceProvider()
                .GetRequiredService<ISystemClock>();

            decoratedClock.UtcNow.Should().Be(t2);
        }
    }
}
