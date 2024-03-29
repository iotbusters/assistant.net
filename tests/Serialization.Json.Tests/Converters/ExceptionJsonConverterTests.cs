using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Exceptions;

namespace Assistant.Net.Serialization.Json.Tests.Converters;

public class ExceptionJsonConverterTests
{
    private readonly JsonSerializerOptions options = new()
    {
        Converters = { Provider.GetRequiredService<ExceptionJsonConverter<SystemException>>() },
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    [Test]
    public void CanConvert_true_exactType() =>
        new ExceptionJsonConverter<SystemException>(null!).CanConvert(typeof(SystemException)).Should().BeTrue();

    [Test]
    public void CanConvert_true_childType() =>
        new ExceptionJsonConverter<SystemException>(null!).CanConvert(typeof(InvalidOperationException)).Should().BeTrue();

    [Test]
    public void CanConvert_false_unrelatedType() =>
        new ExceptionJsonConverter<SystemException>(null!).CanConvert(typeof(Exception)).Should().BeFalse();

    [TestCase("")]
    [TestCase("Some non-json text")]
    [TestCase("{}")]
    [TestCase("{\"message\":\"1\"}")]
    [TestCase("{\"type\":\"InvalidOperationException\"}")]
    public async Task DeserializeInvalidContent(string content)
    {
        await using var stream = new MemoryStream();
        await new StreamWriter(stream).WriteAsync(content);
        stream.Position = 0;

        await this.Awaiting(_ => JsonSerializer.DeserializeAsync<SystemException>(stream, options))
            .Should().ThrowAsync<JsonException>();
    }

    [TestCase("{\"type\":\"Exception\",\"message\":\"1\",\"stacktrace\":\"2\"}")]
    public async Task DeserializeNotMessageExceptionContent(string content)
    {
        await using var stream = new MemoryStream();
        await new StreamWriter(stream).WriteAsync(content);
        stream.Position = 0;

        await this.Awaiting(_ => JsonSerializer.DeserializeAsync<SystemException>(stream, options))
            .Should().ThrowAsync<JsonException>();
    }

    [TestCase("{\"type\":\"InvalidOperationException\",\"message\":\"1\",\"unknown\":\"2\",\"stacktrace\":\"3\"}")]
    public async Task DeserializeAdditionalProperties(string content)
    {
        await using var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;

        var deserialized = await JsonSerializer.DeserializeAsync<SystemException>(stream, options);

        deserialized.Should().BeOfType<InvalidOperationException>()
            .And.BeEquivalentTo(new { Message = "1" });
    }

    [TestCase("{\"type\":\"UnknownException\",\"message\":\"1\",\"stacktrace\":\"2\"}")]
    public async Task DeserializeUnknownException(string content)
    {
        await using var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;

        await this.Awaiting(_ => JsonSerializer.DeserializeAsync<SystemException>(stream, options))
            .Should().ThrowAsync<NotResolvedJsonException>();
    }

    private static IServiceProvider Provider { get; } = new ServiceCollection()
        .AddTypeEncoder()
        .AddTransient<ExceptionJsonConverter<SystemException>>()
        .BuildServiceProvider();
}
