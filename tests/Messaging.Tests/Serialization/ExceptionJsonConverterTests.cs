using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Exceptions;
using Assistant.Net.Messaging.Web.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using FluentAssertions;

namespace Assistant.Net.Messaging.Tests.Exceptions
{
    public class ExceptionJsonConverterTests
    {
        private readonly JsonSerializerOptions options = new()
        {
            Converters = { new ExceptionJsonConverter(Logger) },
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        [TestCase("")]
        [TestCase("Some non-json text")]
        [TestCase("{}")]
        [TestCase("{\"message\":\"1\"}")]
        [TestCase("{\"type\":\"System.Exception, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\"}")]
        public async Task DeserializeInvalidContent(string content)
        {
            using var stream = new MemoryStream();
            await new StreamWriter(stream).WriteAsync(content);
            stream.Position = 0;

            await this.Awaiting(_ => JsonSerializer.DeserializeAsync<Exception>(stream, options))
            .Should().ThrowAsync<JsonException>();
        }

        [TestCase("{\"type\":\"System.Exception, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"message\":\"1\",\"unknown\":\"2\"}")]
        public async Task DeserializeAdditionalProperties(string content)
        {
            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<Exception>(stream, options);

            deserialized.Should().BeOfType<Exception>()
                .And.BeEquivalentTo(new Exception("1"));
        }

        [TestCase("{\"type\":\"UnknownException\",\"message\":\"1\"}")]
        public async Task DeserializeUnknownException(string content)
        {
            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<Exception>(stream, options);

            deserialized.Should().BeOfType<UnknownCommandException>()
                .And.BeEquivalentTo(new UnknownCommandException("UnknownException", "1", null));
        }

        [TestCaseSource(nameof(SupportedExceptions))]
        public async Task DeserializeException(Exception exception)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, exception, options);
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<Exception>(stream, options);

            deserialized.Should().BeOfType(exception.GetType())
                .And.BeEquivalentTo(exception);
        }

        private static IEnumerable<Exception> SupportedExceptions()
        {
            yield return new CommandFailedException("1");
            yield return new CommandFailedException("1", new Exception("2"));
            yield return new CommandRetryLimitExceededException(new Exception("2"));
            yield return new CommandRetryLimitExceededException("1", new Exception("2"));
            yield return new CommandNotRegisteredException("1");
            yield return new CommandNotRegisteredException(typeof(object));
            yield return new CommandConnectionFailedException();
            yield return new CommandConnectionFailedException("1");
        }

        private static ILogger<ExceptionJsonConverter> Logger => NullLogger<ExceptionJsonConverter>.Instance;
    }
}