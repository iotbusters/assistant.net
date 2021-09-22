using Assistant.Net.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Assistant.Net.Core.Tests.Internal
{
    public class TypeEncoderTests
    {
        [TestCaseSource(nameof(ValidCases))]
        public void Encode_returnsName(string name, Type type)
        {
            var encoder = new ServiceCollection()
                .AddTypeEncoder()
                .BuildServiceProvider()
                .GetRequiredService<ITypeEncoder>();

            encoder.Encode(type).Should().Be(name);
        }

        [TestCaseSource(nameof(ValidCases))]
        public void Decode_returnsType(string name, Type type)
        {
            var encoder = new ServiceCollection()
                .AddTypeEncoder()
                .BuildServiceProvider()
                .GetRequiredService<ITypeEncoder>();

            encoder.Decode(name).Should().Be(type);
        }

        [TestCaseSource(nameof(InvalidCases))]
        public void Encode_returnsNull(Type type)
        {
            var encoder = new ServiceCollection()
                .AddTypeEncoder()
                .BuildServiceProvider()
                .GetRequiredService<ITypeEncoder>();

            encoder.Encode(type).Should().BeNull();
        }
        
        public static IEnumerable<TestCaseData> ValidCases() => validTypes.Select(x => new TestCaseData(x.Key, x.Value));
        private static Type[] InvalidCases() => new[] {new {X = 1}.GetType()};

        private static readonly Dictionary<string, Type> validTypes = new()
        {
            ["String"] = typeof(string),
            ["Int32"] = typeof(int),
            ["DateTime"] = typeof(DateTime),
            ["DateTimeOffset"] = typeof(DateTimeOffset),
            ["String[]"] = typeof(string[]),
            ["Int32[,]"] = typeof(int[,]),
            ["IEnumerable`1[Int32]"] = typeof(IEnumerable<int>),
            ["Dictionary`2[Type,Object]"] = typeof(Dictionary<Type, object>),
            ["List`1[IList`1[String]]"] = typeof(List<IList<string>>),
            ["ImmutableArray`1[ImmutableArray`1[String]]"] = typeof(ImmutableArray<ImmutableArray<string>>),
            ["IEnumerable`1"] = typeof(IEnumerable<>)
        };
    }
}
