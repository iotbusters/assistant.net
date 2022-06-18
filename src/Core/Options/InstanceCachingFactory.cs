using System;

namespace Assistant.Net.Options;

/// <summary>
///     Lazily initializing <typeparamref name="TInstance"/> instance factory with caching behavior.
/// </summary>
/// <typeparam name="TInstance">Lazily creating type.</typeparam>
public sealed class InstanceCachingFactory<TInstance> : InstanceFactory<TInstance> where TInstance : class?
{
    private TInstance? instance;

    /// <summary/>
    public InstanceCachingFactory(Func<IServiceProvider, TInstance> factory) : base(factory) { }

    /// <inheritdoc/>
    public override TInstance Create(IServiceProvider provider) => instance ??= Factory(provider);
}

/// <summary>
///     Lazily initializing <typeparamref name="TInstance"/> instance factory with caching behavior.
/// </summary>
/// <typeparam name="TInstance">Lazily creating type.</typeparam>
/// <typeparam name="TParameter">Parameter type required for creating <typeparamref name="TInstance"/> type.</typeparam>
public sealed class InstanceCachingFactory<TInstance, TParameter> : InstanceFactory<TInstance, TParameter> where TInstance : class?
{
    private TInstance? instance;

    /// <summary/>
    public InstanceCachingFactory(Func<IServiceProvider, TParameter, TInstance> factory) : base(factory) { }

    /// <inheritdoc/>
    public override TInstance Create(IServiceProvider provider, TParameter parameter) => instance ??= Factory(provider, parameter);
}
