using Assistant.Net.Core.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Assistant.Net.Core.Tests;

public class ConfigureOptionsSourceTests
{
    [Test]
    public void ChangeOn_notSynchronizesOptionsMonitor_boundToAnotherNamedOptions()
    {
        var source = new TestConfigureOptionsSource<TestOptions2> { ConfigureAction = o => o.Value = "1" };
        var provider = new ServiceCollection()
            .AddOptions<TestOptions1>("name-1")
            .ChangeOn<TestOptions2>("name-2")
            .Configure<IOptionsMonitor<TestOptions2>>((o, m) => o.Value = m.Get("name-2").Value)
            .Services
            .AddOptions<TestOptions2>()
            .Bind(source)
            .Services
            .BuildServiceProvider();

        var options1 = provider.GetRequiredService<IOptionsMonitor<TestOptions1>>();
        var options2 = provider.GetRequiredService<IOptionsMonitor<TestOptions2>>();

        options1.Get("name-1").Value.Should().BeNull();
        options2.Get("name-2").Value.Should().BeNull();
        options2.Get("").Value.Should().Be("1");
    }

    [Test]
    public void ChangeOn_synchronizesOptionsMonitor_onInit()
    {
        var source = new TestConfigureOptionsSource<TestOptions2> { ConfigureAction = o => o.Value = "1" };
        var provider = new ServiceCollection()
            .AddOptions<TestOptions1>("name-1")
            .ChangeOn<TestOptions2>("name-2")
            .Configure<IOptionsMonitor<TestOptions2>>((o, m) => o.Value = m.Get("name-2").Value)
            .Services
            .AddOptions<TestOptions2>("name-2")
            .Bind(source)
            .Services
            .BuildServiceProvider();

        var options1 = provider.GetRequiredService<IOptionsMonitor<TestOptions1>>();
        var options2 = provider.GetRequiredService<IOptionsMonitor<TestOptions2>>();

        options1.Get("name-1").Value.Should().Be("1");
        options2.Get("name-2").Value.Should().Be("1");
    }

    [Test]
    public void ChangeOn_synchronizesOptionsMonitor_onReload()
    {
        var source = new TestConfigureOptionsSource<TestOptions2> { ConfigureAction = o => o.Value = "1" };
        var provider = new ServiceCollection()
            .AddOptions<TestOptions1>("name-1")
            .ChangeOn<TestOptions2>("name-2")
            .Configure<IOptionsMonitor<TestOptions2>>((o, m) => o.Value = m.Get("name-2").Value)
            .Services
            .AddOptions<TestOptions2>("name-2")
            .Bind(source)
            .Services
            .BuildServiceProvider();

        var options1 = provider.GetRequiredService<IOptionsMonitor<TestOptions1>>();
        var options2 = provider.GetRequiredService<IOptionsMonitor<TestOptions2>>();

        options1.Get("name-1").Value.Should().Be("1");
        options2.Get("name-2").Value.Should().Be("1");

        source.ConfigureAction = o => o.Value = "2";
        source.Reload();

        options1.Get("name-1").Value.Should().Be("2");
        options2.Get("name-2").Value.Should().Be("2");
    }

    [Test]
    public void ChangeOn_notSynchronizesOptionsSnapshot_boundToAnotherNamedOptions()
    {
        var source = new TestConfigureOptionsSource<TestOptions2> {ConfigureAction = o => o.Value = "1"};
        var provider = new ServiceCollection()
            .AddOptions<TestOptions1>("name-1")
            .ChangeOn<TestOptions2>("name-2")
            .Configure<IOptionsSnapshot<TestOptions2>>((o, m) => o.Value = m.Get("name-2").Value)
            .Services
            .AddOptions<TestOptions2>()
            .Bind(source)
            .Services
            .BuildServiceProvider();

        var options1 = provider.GetRequiredService<IOptionsSnapshot<TestOptions1>>();
        var options2 = provider.GetRequiredService<IOptionsSnapshot<TestOptions2>>();

        options1.Get("name-1").Value.Should().BeNull();
        options2.Get("name-2").Value.Should().BeNull();
        options2.Get("").Value.Should().Be("1");
    }

    [Test]
    public void ChangeOn_synchronizesOptionsSnapshot_onInit()
    {
        var source = new TestConfigureOptionsSource<TestOptions2> {ConfigureAction = o => o.Value = "1"};
        var provider = new ServiceCollection()
            .AddOptions<TestOptions1>("name-1")
            .ChangeOn<TestOptions2>("name-2")
            .Configure<IOptionsSnapshot<TestOptions2>>((o, m) => o.Value = m.Get("name-2").Value)
            .Services
            .AddOptions<TestOptions2>("name-2")
            .Bind(source)
            .Services
            .BuildServiceProvider();

        var options1 = provider.GetRequiredService<IOptionsSnapshot<TestOptions1>>();
        var options2 = provider.GetRequiredService<IOptionsSnapshot<TestOptions2>>();

        options1.Get("name-1").Value.Should().Be("1");
        options2.Get("name-2").Value.Should().Be("1");
    }

    [Test]
    public void ChangeOn_synchronizesOptionsSnapshot_onReload()
    {
        var source = new TestConfigureOptionsSource<TestOptions2> {ConfigureAction = o => o.Value = "1"};
        var provider = new ServiceCollection()
            .AddOptions<TestOptions1>("name-1")
            .ChangeOn<TestOptions2>("name-2")
            .Configure<IOptionsSnapshot<TestOptions2>>((o, m) => o.Value = m.Get("name-2").Value)
            .Services
            .AddOptions<TestOptions2>("name-2")
            .Bind(source)
            .Services
            .BuildServiceProvider();

        var options1 = provider.GetRequiredService<IOptionsSnapshot<TestOptions1>>();
        var options2 = provider.GetRequiredService<IOptionsSnapshot<TestOptions2>>();

        options1.Get("name-1").Value.Should().Be("1");
        options2.Get("name-2").Value.Should().Be("1");

        source.ConfigureAction = o => o.Value = "2";
        source.Reload();

        options1.Get("name-1").Value.Should().Be("2");
        options2.Get("name-2").Value.Should().Be("2");
    }
}
