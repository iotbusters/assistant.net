using Assistant.Net.Messaging.Interceptors;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Custom message caching abstraction.
/// </summary>
/// <remarks>
///     It impacts <see cref="CachingInterceptor{TMessage,TResponse}"/>.
/// </remarks>
public interface IMessageCacheConfigured
{
    /// <summary>
    ///     Gets caching object to be used as caching id.
    /// </summary>
    object GetCacheId();
}
