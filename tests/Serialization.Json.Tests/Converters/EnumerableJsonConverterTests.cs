using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Converters;
using FluentAssertions;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Json.Tests.Converters
{
    public class EnumerableJsonConverterTests
    {
        private readonly JsonSerializerOptions options = new() {Converters = {new EnumerableJsonConverter()}};

        [TestCase(typeof(string[]), TestName = "CanConvert_true(StringArray)")]
        [TestCase(typeof(ImmutableList<string>), TestName = "CanConvert_true(ImmutableList)")]
        [TestCase(typeof(IReadOnlyCollection<string>), TestName = "CanConvert_true(ReadOnlyCollection)")]
        [TestCase(typeof(IEnumerable<string>), TestName = "CanConvert_true(EnumerableOfString)")]
        [TestCase(typeof(IEnumerable<object>), TestName = "CanConvert_true(EnumerableOfObject)")]
        public void CanConvert_true(Type type) =>
            new EnumerableJsonConverter().CanConvert(type).Should().BeTrue();

        [TestCase(typeof(Array), TestName = "CanConvert_false(Array)")]
        [TestCase(typeof(ImmutableList), TestName = "CanConvert_false(ImmutableList)")]
        [TestCase(typeof(IEnumerable), TestName = "CanConvert_false(Enumerable)")]
        [TestCase(typeof(string), TestName = "CanConvert_false(String)")]
        [TestCase(typeof(object), TestName = "CanConvert_false(Object)")]
        public void CanConvert_false(Type type) =>
            new EnumerableJsonConverter().CanConvert(type).Should().BeFalse();

        [TestCaseSource(nameof(SupportedObjects))]
        public async Task DeserializeObject(object value)
        {
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, value, options);
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync(stream, value.GetType(), options);

            deserialized.Should().BeOfType(value.GetType())
                .And.BeEquivalentTo(value);
        }

        private static IEnumerable<TestCaseData> SupportedObjects()
        {
            var values = new[] {"1", "2", "3"};
            yield return new TestCaseData((object)values){TestName = "DeserializeObject(StringArray)" };
            yield return new TestCaseData(values.ToList()) { TestName = "DeserializeObject(List)" };
            yield return new TestCaseData(values.ToDictionary(x => x)) { TestName = "DeserializeObject(Dictionary)" };
            yield return new TestCaseData(Encoding.UTF8.GetBytes("123")) { TestName = "DeserializeObject(ByteArray)" };
        }
    }
}