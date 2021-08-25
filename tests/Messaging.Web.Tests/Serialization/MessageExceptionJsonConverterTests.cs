using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests.Serialization
{
    public class MessageExceptionJsonConverterTests
    {
        private readonly JsonSerializerOptions options = new()
        {
            Converters = { Provider.GetRequiredService<MessageExceptionJsonConverter>() },
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        [TestCase("")]
        [TestCase("Some non-json text")]
        [TestCase("{}")]
        [TestCase("{\"message\":\"1\"}")]
        [TestCase("{\"type\":\"MessageFailedException\"}")]
        public async Task DeserializeInvalidContent(string content)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            stream.Position = 0;

            await this.Awaiting(_ => JsonSerializer.DeserializeAsync<MessageException>(stream, options))
            .Should().ThrowAsync<JsonException>();
        }

        [TestCase("{\"type\":\"Exception\",\"message\":\"1\"}")]
        public async Task DeserializeNotMessageExceptionContent(string content)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            stream.Position = 0;

            await this.Awaiting(_ => JsonSerializer.DeserializeAsync<MessageException>(stream, options))
                .Should().ThrowAsync<JsonException>();
        }

        [TestCase("{\"type\":\"MessageFailedException\",\"message\":\"1\",\"unknown\":\"2\"}")]
        public async Task DeserializeAdditionalProperties(string content)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<MessageException>(stream, options);

            deserialized.Should().BeOfType<MessageFailedException>()
                .And.BeEquivalentTo(new { Message = "1" });
        }

        [TestCase("{\"type\":\"UnknownException\",\"message\":\"1\"}")]
        public async Task DeserializeUnknownException(string content)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<MessageException>(stream, options);

            deserialized.Should().BeOfType<UnknownMessageException>()
                .And.BeEquivalentTo(new UnknownMessageException("UnknownException", "1", null));
        }

        [TestCaseSource(nameof(SupportedExceptions))]
        public async Task DeserializeException(MessageException exception)
        {
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, exception, options);
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<MessageException>(stream, options);

            deserialized.Should().BeOfType(exception.GetType())
                .And.BeEquivalentTo(new { exception.Message, InnerException = (Exception?)null });
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

        private static IServiceProvider Provider { get; } = new ServiceCollection()
            .AddTypeEncoder()
            .AddTransient<MessageExceptionJsonConverter>()
            .BuildServiceProvider();
    }
}