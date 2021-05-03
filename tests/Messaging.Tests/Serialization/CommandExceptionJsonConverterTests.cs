using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Serialization;
using NUnit.Framework;
using FluentAssertions;

namespace Assistant.Net.Messaging.Tests.Exceptions
{
    public class CommandExceptionJsonConverterTests
    {
        private readonly JsonSerializerOptions options = new()
        {
            Converters = { new CommandExceptionJsonConverter(Logger) },
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        //Assistant.Net.Messaging.Exceptions.CommandFailedException, assistant.net.messaging.inmemory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
        [TestCase("")]
        [TestCase("Some non-json text")]
        [TestCase("{}")]
        [TestCase("{\"message\":\"1\"}")]
        [TestCase("{\"type\":\"Assistant.Net.Messaging.Exceptions.CommandFailedException, assistant.net.messaging.inmemory\"}")]
        public async Task DeserializeInvalidContent(string content)
        {
            using var stream = new MemoryStream();
            await new StreamWriter(stream).WriteAsync(content);
            stream.Position = 0;

            await this.Awaiting(_ => JsonSerializer.DeserializeAsync<CommandException>(stream, options))
            .Should().ThrowAsync<JsonException>();
        }

        [TestCase("{\"type\":\"System.Exception, System.Private.CoreLib\",\"message\":\"1\"}")]
        public async Task DeserializeNotCommandExceptionContent(string content)
        {
            using var stream = new MemoryStream();
            await new StreamWriter(stream).WriteAsync(content);
            stream.Position = 0;

            await this.Awaiting(_ => JsonSerializer.DeserializeAsync<CommandException>(stream, options))
                .Should().ThrowAsync<JsonException>();
        }

        [TestCase("{\"type\":\"Assistant.Net.Messaging.Exceptions.CommandFailedException, assistant.net.messaging.inmemory\",\"message\":\"1\",\"unknown\":\"2\"}")]
        public async Task DeserializeAdditionalProperties(string content)
        {
            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<CommandException>(stream, options);

            deserialized.Should().BeOfType<CommandFailedException>()
                .And.BeEquivalentTo(new { Message = "1" });
        }

        [TestCase("{\"type\":\"UnknownException\",\"message\":\"1\"}")]
        public async Task DeserializeUnknownException(string content)
        {
            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<CommandException>(stream, options);

            deserialized.Should().BeOfType<UnknownCommandException>()
                .And.BeEquivalentTo(new UnknownCommandException("UnknownException", "1", null));
        }

        [TestCaseSource(nameof(SupportedExceptions))]
        public async Task DeserializeException(CommandException exception)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, exception, options);
            stream.Position = 0;

            var deserialized = await JsonSerializer.DeserializeAsync<CommandException>(stream, options);

            deserialized.Should().BeOfType(exception.GetType())
                .And.BeEquivalentTo(new { exception.Message, InnerException = (Exception?)null });
        }

        private static IEnumerable<CommandException> SupportedExceptions()
        {
            yield return new CommandFailedException("1");
            yield return new CommandFailedException("1", new Exception("2"));
            yield return new CommandRetryLimitExceededException();
            yield return new CommandRetryLimitExceededException("1");
            yield return new CommandRetryLimitExceededException("1", new Exception("2"));
            yield return new CommandNotRegisteredException("1");
            yield return new CommandNotRegisteredException(typeof(object));
            yield return new CommandConnectionFailedException();
            yield return new CommandConnectionFailedException("1");
        }

        private static ILogger<CommandExceptionJsonConverter> Logger => NullLogger<CommandExceptionJsonConverter>.Instance;
    }
}