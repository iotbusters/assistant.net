using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Json.Tests.Mocks;

namespace Assistant.Net.Serialization.Json.Tests.Converters;

public class EnumerableJsonConverterTests
{
    private static readonly IServiceProvider Provider = new ServiceCollection().BuildServiceProvider();

    [TestCase(typeof(string), TestName = "NewConverterOfSystemType_failed(String)")]
    [TestCase(typeof(int), TestName = "NewConverterOfSystemType_failed(Int32)")]
    [TestCase(typeof(byte), TestName = "NewConverterOfSystemType_failed(Byte)")]
    [TestCase(typeof(DateTime), TestName = "NewConverterOfSystemType_failed(DateTime)")]
    [TestCase(typeof(object), TestName = "NewConverterOfSystemType_failed(Object)")]
    [TestCase(typeof(List<string>), TestName = "NewConverterOfSystemType_failed(List)")]
    [TestCase(typeof(IEnumerable<string>), TestName = "NewConverterOfSystemType_failed(Enumerable)")]
    [TestCase(typeof(Exception), TestName = "NewConverterOfSystemType_failed(Exception)")]
    [TestCase(typeof(Type), TestName = "NewConverterOfSystemType_failed(Type)")]
    public void NewInstanceOfSystemType_failed(Type systemType)
    {
        this.Invoking(_ => (JsonConverter)ActivatorUtilities.CreateInstance(
                Provider,
                typeof(EnumerableJsonConverter<>).MakeGenericType(systemType)))
            .Should().Throw<JsonException>()
            .WithMessage($"The type '{systemType}' isn't supported.");
    }

    [TestCase(typeof(TestClass[]), TestName = "CanConvert_true(Array)")]
    [TestCase(typeof(ImmutableList<TestClass>), TestName = "CanConvert_true(ImmutableList)")]
    [TestCase(typeof(IReadOnlyCollection<TestClass>), TestName = "CanConvert_true(ReadOnlyCollection)")]
    [TestCase(typeof(IEnumerable<TestClass>), TestName = "CanConvert_true(Enumerable)")]
    public void CanConvert_true(Type type)
    {
        new EnumerableJsonConverter<TestClass>().CanConvert(type).Should().BeTrue();
    }

    [TestCase(typeof(Array), TestName = "CanConvert_false(Array)")]
    [TestCase(typeof(ImmutableList), TestName = "CanConvert_false(ImmutableList)")]
    [TestCase(typeof(IEnumerable), TestName = "CanConvert_false(Enumerable)")]
    [TestCase(typeof(string[]), TestName = "CanConvert_false(StringArray)")]
    [TestCase(typeof(ImmutableList<string>), TestName = "CanConvert_false(ImmutableListOfString)")]
    [TestCase(typeof(IReadOnlyCollection<string>), TestName = "CanConvert_false(ReadOnlyCollectionOfString)")]
    [TestCase(typeof(IEnumerable<string>), TestName = "CanConvert_false(EnumerableOfString)")]
    public void CanConvert_false(Type type)
    {
        new EnumerableJsonConverter<TestClass>().CanConvert(type).Should().BeFalse();
    }

    [TestCaseSource(nameof(SupportedObjects))]
    public async Task DeserializeObject(object value)
    {
        var options = new JsonSerializerOptions { Converters = { new EnumerableJsonConverter<TestClass>() } };

        await using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, options);
        stream.Position = 0;

        var deserialized = await JsonSerializer.DeserializeAsync(stream, typeof(IEnumerable<TestClass>), options);

        deserialized.Should().BeEquivalentTo(value);
    }

    private static IEnumerable<TestCaseData> SupportedObjects()
    {
        var values = new[] {new TestClass(DateTime.UtcNow)};
        yield return new((object)values){TestName = "DeserializeObject(Array)" };
        yield return new(values.ToList()) { TestName = "DeserializeObject(List)" };
        yield return new(values.ToHashSet()) { TestName = "DeserializeObject(HashSet)" };
        yield return new(new Stack<TestClass>(values)) { TestName = "DeserializeObject(Stack)" };
        yield return new(new Queue<TestClass>(values)) { TestName = "DeserializeObject(Queue)" };
    }
}
