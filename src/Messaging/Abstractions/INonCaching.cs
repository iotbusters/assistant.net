using Assistant.Net.Messaging.Interceptors;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Ignoring message caching interface marker.
/// </summary>
/// <remarks>
///     It impacts <see cref="CachingInterceptor{TMessage,TResponse}"/>.
/// </remarks>
public interface INonCaching { }
