namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An extended abstraction over storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>
    ///     including value change history that exposes all stored keys.
    /// </summary>
    /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
    /// <typeparam name="TValue">A value object type is stored.</typeparam>
    public interface IHistoricalAdminStorage<TKey, TValue> : IHistoricalStorage<TKey, TValue>, IAdminStorage<TKey, TValue> { }
}
