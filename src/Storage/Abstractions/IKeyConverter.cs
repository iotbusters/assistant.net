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
        /// <param name="key">Specific key object.</param>
        StoreKey Convert(TKey key);


        /// <summary>
        ///     Converts <paramref name="key"/> to specific key object.
        /// </summary>
        /// <param name="key">Internal key object.</param>
        TKey Convert(StoreKey key);
    }
}