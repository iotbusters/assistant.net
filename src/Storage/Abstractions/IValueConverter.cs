namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An abstraction over a value conversion for <see cref="IBinaryStorageProvider" /> implementation.
    /// </summary>
    /// <typeparam name="TKey">Value object type.</typeparam>
    public interface IValueConverter<TValue>
    {
        /// <summary>
        ///     Converts <paramref name="value"/> object to binary.
        /// </summary>
        /// <param name="value">Value object.</param>
        byte[] Convert(TValue value);

        /// <summary>
        ///     Converts <paramref name="value"/> binary to object.
        /// </summary>
        /// <param name="value">Value binary.</param>
        TValue Convert(byte[] value);
    }
}