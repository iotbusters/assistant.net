using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Serialization;

public class MessageExceptionJsonConverterTests
{

    [TestCase("", "The input does not contain any JSON tokens. Expected the input to start with a valid JSON token, when isFinalBlock is true. Path: $ | LineNumber: 0 | BytePositionInLine: 0.")]
    [TestCase("Some non-json text", "'S' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.")]
    [TestCase("{}", "Property 'type' is required.")]
    [TestCase("{\"message\":\"1\"}", "Property 'type' is required.")]
    [TestCase("{\"type\":\"MessageFailedException\"}", "Property 'message' is required.")]
    public async Task DeserializeAsync_throwsJsonException_invalidContent(string content, string message)
    {
        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;

        await this.Awaiting(_ => JsonSerializer.DeserializeAsync<MessageException>(stream, Options()))
            .Should().ThrowAsync<JsonException>().WithMessage(message);
    }

    [TestCase("{\"type\":\"Exception\",\"message\":\"1\"}")]
    public async Task DeserializeAsync_throwsJsonException_notMessageExceptionContent(string content)
    {
        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;

        await this.Awaiting(_ => JsonSerializer.DeserializeAsync<MessageException>(stream, Options()))
            .Should().ThrowAsync<JsonException>().WithMessage("Unsupported by converter exception type `Exception`.");
    }

    [TestCase("{\"type\":\"MessageFailedException\",\"message\":\"1\",\"unknown\":\"2\"}")]
    public async Task DeserializeAsync_returnsMessageFailedException_additionalProperties(string content)
    {
        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;

        var deserialized = await JsonSerializer.DeserializeAsync<MessageException>(stream, Options());

        deserialized.Should().BeOfType<MessageFailedException>()
            .And.BeEquivalentTo(new { Message = "1" });
    }

    [TestCase("{\"type\":\"UnknownException1\",\"message\":\"1\"}")]
    public async Task DeserializeAsync_returnsUnknownMessageException_unknownException(string content)
    {
        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;

        var deserialized = await JsonSerializer.DeserializeAsync<MessageException>(stream, Options());

        deserialized.Should().BeOfType<UnknownMessageException>()
            .And.BeEquivalentTo(new UnknownMessageException("UnknownException1", "1", null));
    }

    [TestCaseSource(nameof(SupportedExceptions))]
    public async Task DeserializeAsync_returnsSupportedExceptionWithoutInnerException(MessageException exception)
    {
        var options = Options();
        await using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, exception, options);
        stream.Position = 0;

        var deserialized = await JsonSerializer.DeserializeAsync<MessageException>(stream, options);

        deserialized.Should().BeOfType(exception.GetType())
            .And.BeEquivalentTo(new {exception.Message, InnerException = (Exception?)null});
    }

    [TestCaseSource(nameof(SupportedExceptions))]
    public async Task DeserializeAsync_returnsSupportedExceptionIncludingInnerException(MessageException exception)
    {
        var options = Options(typeof(Exception));
        await using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, exception, options);
        stream.Position = 0;

        var deserialized = await JsonSerializer.DeserializeAsync<MessageException>(stream, options);

        deserialized.Should().BeOfType(exception.GetType())
            .And.BeEquivalentTo(exception);
    }

    private static IEnumerable<MessageException> SupportedExceptions()
    {
        yield return new MessageFailedException("1");
        yield return new MessageFailedException("1", new Exception("2"));
        yield return new MessageRetryLimitExceededException();
        yield return new MessageRetryLimitExceededException("1");
        yield return new MessageRetryLimitExceededException("1", new Exception("2"));
        yield return new MessageNotRegisteredException("1");
        yield return new MessageNotRegisteredException(typeof(object));
        yield return new MessageConnectionFailedException();
        yield return new MessageConnectionFailedException("1");
    }

    private JsonSerializerOptions Options(Type? exceptionType = null) => new()
    {
        Converters =
        {
            new ServiceCollection()
                .AddTypeEncoder()
                .AddTransient<MessageExceptionJsonConverter>()
                .AddSingleton<INamedOptions<MessagingClientOptions>>(new TestNamedOptions
                {
                    Value = new MessagingClientOptions {ExposedExceptions = {exceptionType ?? typeof(NullReferenceException)}}
                })
                .BuildServiceProvider()
                .GetRequiredService<MessageExceptionJsonConverter>()
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
}
