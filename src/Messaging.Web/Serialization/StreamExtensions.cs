using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Common operations over a <see cref="Stream" /> object.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        ///     Deserialize <see cref="CommandException" /> object from streamed json.
        /// </summary>
        public static Task<CommandException?> ReadException(this Stream stream, JsonSerializerOptions options, CancellationToken cancellationToken) =>
            stream.ReadObject<CommandException>(options, cancellationToken);

        /// <summary>
        ///     Deserialize <typeparamref name="T"/> object from streamed json.
        /// </summary>
        public static async Task<T?> ReadObject<T>(this Stream stream, JsonSerializerOptions options, CancellationToken cancellationToken)
            where T : class => (T?)await stream.ReadObject(typeof(T), options, cancellationToken);

        /// <summary>
        ///     Deserialize <paramref name="objectType"/> object from streamed json.
        /// </summary>
        public static async Task<object?> ReadObject(this Stream stream, Type objectType, JsonSerializerOptions options, CancellationToken cancellationToken)
        {
            try
            {
                return await JsonSerializer.DeserializeAsync(stream, objectType, options, cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new CommandContractException("Failed to parse invalid content.", ex);
            }
        }
    }
}