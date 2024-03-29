﻿using Assistant.Net.Messaging.Interceptors;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Ignoring message caching interface marker.
/// </summary>
/// <remarks>
///     It impacts <see cref="CachingInterceptor"/>.
/// </remarks>
public interface INonCaching { }
