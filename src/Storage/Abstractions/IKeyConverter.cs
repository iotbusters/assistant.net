namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An abstraction over a key conversion for <see cref="IBinaryStorage" /> implementation.
    /// </summary>
    /// <typeparam name="TKey">Key object type.</typeparam>
    public interface IKeyConverter<TKey>
    {
        /// <summary>
        ///     Converts <paramref name="key"/> object to string value.
        /// </summary>
        /// <param name="key">Key object.</param>
        string Convert(TKey key);
    }
}