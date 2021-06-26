using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Json.Tests.Mocks;

namespace Assistant.Net.Serialization.Json.Tests.Converters
{
    public class AdvancedJsonConverterTests
    {
        private readonly JsonSerializerOptions options = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false),
                new EnumerableJsonConverter(),
                Provider.GetRequiredService<AdvancedJsonConverter>()
            }
        };

        private static IServiceProvider Provider { get; } = new ServiceCollection()
            .AddTypeEncoder()
            .AddTransient<AdvancedJsonConverter>()
            .BuildServiceProvider();

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
            yield return new TestCaseData(
                    new TestObjectWithCtorInitialization(
                        TestEnum.A,
                        "test",
                        11,
                        11.1f,
                        11.11m,
                        DateTime.Now,
                        new int?[] {1},
                        new[] {"test"}.AsEnumerable(),
                        new[] {new TestClass(DateTime.Now)}))
                {TestName = "InitializeByCtor"};

            yield return new TestCaseData(
                    new TestObjectWithCtorInitialization(
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        new int?[] {1},
                        new[] {"test"}.AsEnumerable(),
                        new[] {new TestClass(DateTime.Now)}))
                {TestName = "InitializeByCtor_nullable"};

            yield return new TestCaseData(
                    new TestObjectWithCtorInitialization(
                        TestEnum.A,
                        "test",
                        11,
                        11.1f,
                        11.11m,
                        DateTime.Now,
                        new int?[] {null, 11},
                        new[] {null, "test"}.AsEnumerable(),
                        new[] {null, new TestClass(DateTime.Now)}))
                {TestName = "InitializeByCtor_nullableArrays"};

            yield return new TestCaseData(
                    new TestObjectWithPropertyInitialization
                    {
                        Enum = TestEnum.A,
                        String = "test",
                        IntegerNumber = 11,
                        FloatNumber = 11.1f,
                        DecimalNumber = 11.11m,
                        DateTime = DateTime.Now,
                        StringArray = new[] {"test"}.AsEnumerable(),
                        ObjectArray = new[] {new TestClass(DateTime.Now)}
                    })
                {TestName = "InitializeBySetters"};

            yield return new TestCaseData(
                    new TestObjectWithPropertyInitialization
                    {
                        StringArray = new[] {"test"}.AsEnumerable(),
                        ObjectArray = new[] {new TestClass(DateTime.Now)}
                    })
                {TestName = "InitializeBySetters_nullable"};

            yield return new TestCaseData(
                    new TestObjectWithPropertyInitialization
                    {
                        Enum = TestEnum.A,
                        String = "test",
                        IntegerNumber = 11,
                        FloatNumber = 11.1f,
                        DecimalNumber = 11.11m,
                        DateTime = DateTime.Now,
                        IntegerArray = new int?[] {null, 11},
                        StringArray = new[] {null, "test"}.AsEnumerable(),
                        ObjectArray = new[] {null, new TestClass(DateTime.Now)}
                    })
                {TestName = "InitializeBySetters_nullableArrays"};

            yield return new TestCaseData(
                    new TestObjectWithMixedInitialization(TestEnum.A)
                    {
                        String = "test"
                    })
                {TestName = "InitializeByCtorAndSetters"};

            yield return new TestCaseData(
                    new TestObjectWithMixedInitialization(null)
                    {
                        String = "test"
                    })
                {TestName = "InitializeByCtorAndSetters_defaultValue"};

            yield return new TestCaseData(
                    new TestObjectWithTypeCastingInitialization(TestEnum.A, 11, 12, new[] {"test"}))
                {TestName = "Initialize_typeCasting"};

            yield return new TestCaseData(
                    new TestObjectWithTypeCastingInitialization(TestEnum.A, null, 12, new[] {"test"}))
                {TestName = "Initialize_typeCastingAndDefaultValue"};
        }
    }
}