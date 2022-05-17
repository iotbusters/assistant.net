using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Unions;

namespace Assistant.Net.Core.Tests;

public class OptionTests
{
    [Test]
    public void Some()
    {
        Option<int> option = Option.Some(111);
        var value = option switch
        {
            Some<int>(var x) => x,
            _                => default
        };

        value.Should().Be(111);
    }

    [Test]
    public void None()
    {
        Option<int> option = Option.None;
        var value = option switch
        {
            None<int> => true,
            _         => false
        };

        value.Should().BeTrue();
    }

    [Test]
    public void Option_boolean()
    {
        ((bool)Option.None).Should().BeFalse();
        ((bool)Option.None).Should().BeFalse();
        ((bool)Option.Some(1)).Should().BeTrue();
    }

    [Test]
    public void Some_equals_Some()
    {
        Option.Some("test").Equals(Option.Some("test")).Should().BeTrue();
    }

    [Test]
    public void Some_unequals_Some()
    {
        Option.Some("test-1").Equals(Option.Some("test-2")).Should().BeFalse();
    }

    [Test]
    public void Some_unequals_None()
    {
        Option.Some("test").Equals(Option.None).Should().BeFalse();
    }

    [Test]
    public void SomeOfString_unequals_SomeOfObject()
    {
        object option1 = Option.Some("test");
        object option2 = Option.Some<object>("test");
        option1.Equals(option2).Should().BeFalse();
    }
}
