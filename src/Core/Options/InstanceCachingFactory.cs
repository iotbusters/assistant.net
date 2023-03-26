using System;
using System.Collections.Generic;

namespace Assistant.Net.Options;

/// <summary>
///     Lazily initializing <typeparamref name="TInstance" /> instance factory with caching behavior.
/// </summary>
/// <typeparam name="TInstance">Lazily creating type.</typeparam>
public sealed class InstanceCachingFactory<TInstance> : InstanceFactory<TInstance> where TInstance : class?
{
    private TInstance? instance;

    /// <summary />
    public InstanceCachingFactory(Func<IServiceProvider, TInstance> factory) : base(factory) { }

    /// <inheritdoc />
    public override TInstance Create(IServiceProvider provider)
    {
        return instance ??= Factory(provider);
    }
}

/// <summary>
///     Lazily initializing <typeparamref name="TInstance" /> instance factory with caching behavior.
/// </summary>
/// <typeparam name="TInstance">Lazily creating type.</typeparam>
/// <typeparam name="TParameter">Parameter type required for creating <typeparamref name="TInstance" /> type.</typeparam>
public sealed class InstanceCachingFactory<TInstance, TParameter> : InstanceFactory<TInstance, TParameter>
    where TInstance : class?
    where TParameter : notnull
{
    private readonly Dictionary<TParameter, TInstance> instances = new();

    /// <summary />
    public InstanceCachingFactory(Func<IServiceProvider, TParameter, TInstance> factory) : base(factory) { }

    /// <inheritdoc />
    public override TInstance Create(IServiceProvider provider, TParameter parameter)
    {
        if (instances.TryGetValue(parameter, out var instance))
            return instance;
        return instances[parameter] = Factory(provider, parameter);
    }
}
