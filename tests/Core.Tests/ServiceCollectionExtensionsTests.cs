using Assistant.Net.Abstractions;
using Assistant.Net.Core.Tests.Mocks;
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
        public void Decorate_interceptsOriginalCall_factorySetup()
        {
            var time = new DateTimeOffset(new DateTime(2001, 1, 1));
            var decoratedClock = new ServiceCollection()
                .AddSingleton<ISystemClock>(_ => new TestClock())
                .Decorate<ISystemClock>(proxy => proxy.Intercept(x => x.UtcNow, _ => time))
                .BuildServiceProvider()
                .GetRequiredService<ISystemClock>();

            decoratedClock.UtcNow.Should().Be(time);
        }

        [Test]
        public void Decorate_interceptsOriginalCall_typeSetup()
        {
            var time = new DateTimeOffset(new DateTime(2001, 1, 1));
            var decoratedClock = new ServiceCollection()
                .AddSingleton<ISystemClock, TestClock>()
                .Decorate<ISystemClock>(proxy => proxy.Intercept(x => x.UtcNow, _ => time))
                .BuildServiceProvider()
                .GetRequiredService<ISystemClock>();

            decoratedClock.UtcNow.Should().Be(time);
        }

        [Test]
        public void Decorate_interceptsOriginalCall_instanceSetup()
        {
            var time = new DateTimeOffset(new DateTime(2001, 1, 1));
            var decoratedClock = new ServiceCollection()
                .AddSingleton<ISystemClock>(new TestClock())
                .Decorate<ISystemClock>(proxy => proxy.Intercept(x => x.UtcNow, _ => time))
                .BuildServiceProvider()
                .GetRequiredService<ISystemClock>();

            decoratedClock.UtcNow.Should().Be(time);
        }
    }
}
