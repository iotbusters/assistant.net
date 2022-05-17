using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

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
    /// <param name="token"/>
    Task<byte[]> Convert(TValue value, CancellationToken token = default);

    /// <summary>
    ///     Converts <paramref name="value"/> binary to object.
    /// </summary>
    /// <param name="value">Value binary.</param>
    /// <param name="token"/>
    Task<TValue> Convert(byte[] value, CancellationToken token = default);
}
