using Assistant.Net.Messaging.Interceptors;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Ignoring message caching abstraction.
/// </summary>
/// <remarks>
///     It impacts <see cref="CachingInterceptor{TMessage,TResponse}"/>.
/// </remarks>
public interface IMessageCacheIgnored { }
