
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Serialization;

namespace Assistant.Net.Messaging.Web.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddJsonSerializerOptions_registersServiceDescriptors()
        {
            var services = new ServiceCollection().AddJsonSerializerOptions(_ => { });

            services.Should().ContainEquivalentOf(new
            {
                ServiceType = typeof(CommandExceptionJsonConverter),
                ImplementationType = typeof(CommandExceptionJsonConverter)
            });
        }

        [Test]
        public void GetServiceOfCommandExceptionJsonConverter_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddJsonSerializerOptions(_ => { })
                .BuildServiceProvider();

            provider.GetService<CommandExceptionJsonConverter>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfITypeEncoder_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddJsonSerializerOptions(_ => { })
                .BuildServiceProvider();

            provider.GetService<ITypeEncoder>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfOptions_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddJsonSerializerOptions(_ => { })
                .BuildServiceProvider();

            provider.GetService<IOptions<JsonSerializerOptions>>()
                .Should().NotBeNull();
        }
    }
}