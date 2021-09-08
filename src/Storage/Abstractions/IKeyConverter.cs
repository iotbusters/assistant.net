using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An abstraction over key conversion for <see cref="IStorageProvider{TValue}"/>
    ///     and <see cref="IPartitionedStorageProvider{TValue}"/> implementations.
    /// </summary>
    /// <typeparam name="TKey">Key object type.</typeparam>
    public interface IKeyConverter<TKey>
    {
        /// <summary>
        ///     Converts <paramref name="key"/> to internal key presentation object.
        /// </summary>
        /// <param name="key"> A specific key object.</param>
        /// <param name="token">A cancellation token.</param>
        Task<byte[]> Convert(TKey key, CancellationToken token = default);


        /// <summary>
        ///     Converts <paramref name="keyContent"/> to specific key object.
        /// </summary>
        /// <param name="keyContent">A key binary content.</param>
        /// <param name="token">A cancellation token.</param>
        Task<TKey> Convert(byte[] keyContent, CancellationToken token = default);
    }
}