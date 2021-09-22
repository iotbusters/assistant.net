using Assistant.Net.Diagnostics.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Assistant.Net.Diagnostics.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void GetServiceOfIDiagnosticContext_resolvesObject()
        {
            var context = new ServiceCollection()
                .AddDiagnosticContext()
                .BuildServiceProvider()
                .GetService<IDiagnosticContext>();

            context.Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfIDiagnosticFactory_resolvesObject()
        {
            var context = new ServiceCollection()
                .AddDiagnostics()
                .BuildServiceProvider()
                .GetService<IDiagnosticFactory>();

            context.Should().NotBeNull();
        }
    }
}
