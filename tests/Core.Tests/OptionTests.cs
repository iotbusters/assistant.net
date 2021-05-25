using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Unions;

namespace Assistant.Net.Tests
{
    public class OptionTests
    {
        [Test]
        public void Some()
        {
            Option<int> option = Option.Some(111);
            var value = option switch
            {
                Some<int>(var x) => x,
                _ => default
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
                _ => false
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
    }
}