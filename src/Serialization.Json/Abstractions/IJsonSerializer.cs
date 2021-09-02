using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Abstractions
{
    /// <summary>
    ///     An abstraction over common purpose JSON serializer.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        ///     Serializes <paramref name="value"/> object as JSON to <paramref name="stream"/>.
        /// </summary>
        Task Serialize(Stream stream, object value, CancellationToken token = default);

        /// <summary>
        ///     Deserializes JSON from <paramref name="stream"/> to <paramref name="type" /> object.
        /// </summary>
        Task<object> Deserialize(Stream stream, Type type, CancellationToken token = default);
    }
}