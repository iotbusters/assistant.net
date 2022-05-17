using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Assistant.Net.Diagnostics.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void GetServiceOfIDiagnosticContext_resolvesObject_calledAddDiagnosticContext()
    {
        var context = new ServiceCollection()
            .AddDiagnosticContext()
            .BuildServiceProvider()
            .GetService<IDiagnosticContext>();

        context.Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfIDiagnosticContext_resolvesObject_calledAddDiagnosticContextWithFunc()
    {
        var context = new ServiceCollection()
            .AddDiagnosticContext(_ => string.Empty)
            .BuildServiceProvider()
            .GetService<IDiagnosticContext>();

        context.Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfIDiagnosticContext_resolvesObject_calledAddDiagnosticContextOfTestDiagnosticContext()
    {
        var context = new ServiceCollection()
            .AddDiagnosticContext<TestDiagnosticContext>()
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
