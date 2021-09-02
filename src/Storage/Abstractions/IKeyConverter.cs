using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An abstraction over a key conversion for <see cref="IStorageProvider{TValue}" /> implementation.
    /// </summary>
    /// <typeparam name="TKey">Key object type.</typeparam>
    public interface IKeyConverter<TKey>
    {
        /// <summary>
        ///     The only supported <see cref="StoreKey.Type" />.
        /// </summary>
        string KeyType { get; }

        /// <summary>
        ///     Converts <paramref name="key"/> to internal key presentation object.
        /// </summary>
        /// <param name="key"> A specific key object.</param>
        /// <param name="token">A cancellation token.</param>
        Task<StoreKey> Convert(TKey key, CancellationToken token = default);


        /// <summary>
        ///     Converts <paramref name="key"/> to specific key object.
        /// </summary>
        /// <param name="key">An internal key object.</param>
        /// <param name="token">A cancellation token.</param>
        Task<TKey> Convert(StoreKey key, CancellationToken token = default);
    }
}