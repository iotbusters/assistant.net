using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An abstraction over a value conversion for a storage implementation.
    /// </summary>
    /// <typeparam name="TValue">Value object type.</typeparam>
    public interface IValueConverter<TValue>
    {
        /// <summary>
        ///     Converts <paramref name="value"/> object to binary.
        /// </summary>
        /// <param name="value">Value object.</param>
        Task<byte[]> Convert(TValue value);

        /// <summary>
        ///     Converts <paramref name="value"/> binary to object.
        /// </summary>
        /// <param name="value">Value binary.</param>
        Task<TValue> Convert(byte[] value);
    }
}