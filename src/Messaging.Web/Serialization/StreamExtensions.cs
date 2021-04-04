using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Serialization
{
    public static class StreamExtensions
    {

        public static async Task<T?> ReadObject<T>(this Stream stream, JsonSerializerOptions options, CancellationToken cancellationToken)
            where T : class => (T?)await stream.ReadObject(typeof(T), options, cancellationToken);

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