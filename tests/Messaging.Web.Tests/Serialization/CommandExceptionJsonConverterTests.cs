using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Serialization;

namespace Assistant.Net.Messaging.Web.Tests.Serialization
{
    public class CommandExceptionJsonConverterTests
    {
        private readonly JsonSerializerOptions options = new()
        {
            Converters = { Provider.GetRequiredService<CommandExceptionJsonConverter>() },
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        [TestCase("")]
        [TestCase("Some non-json text")]
        [TestCase("{}")]
        [TestCase("{\"message\":\"1\"}")]
        [TestCase("{\"type\":\"CommandFailedException\"}")]
        public async Task DeserializeInvalidContent(string content)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            stream.Position = 0;

            await this.Awaiting(_ => JsonSerializer.DeserializeAsync<CommandException>(stream, options))
            .Should().ThrowAsync<JsonException>();
        }

        [TestCase("{\"type\":\"Exception\",\"message\":\"1\"}")]
        public async Task DeserializeNotCommandExceptionContent(string content)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            stream.Position = 0;

            await this.Awaiting(_ => JsonSerializer.DeserializeAsync<CommandException>(stream, options))
                .Should().ThrowAsync<JsonException>();
        }

        [TestCase("{\"type\":\"CommandFailedException\",\"message\":\"1\",\"unknown\":\"2\"}")]
        public async Task DeserializeAdditionalProperties(string content)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
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
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
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
            await using var stream = new MemoryStream();
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

        private static IServiceProvider Provider { get; } = new ServiceCollection()
            .AddTypeEncoder()
            .AddTransient<CommandExceptionJsonConverter>()
            .BuildServiceProvider();
    }
}