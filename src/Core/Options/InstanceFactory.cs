using System;

namespace Assistant.Net.Options;

/// <summary>
///     Lazily initializing <typeparamref name="TInstance"/> instance factory.
/// </summary>
/// <typeparam name="TInstance">Lazily creating type.</typeparam>
public class InstanceFactory<TInstance> where TInstance : class?
{
    /// <summary>
    ///     Factory method.
    /// </summary>
    protected Func<IServiceProvider, TInstance> Factory { get; }

    /// <summary/>
    public InstanceFactory(Func<IServiceProvider, TInstance> factory) =>
        this.Factory = factory;

    /// <summary>
    ///     Creates an instance.
    /// </summary>
    public virtual TInstance Create(IServiceProvider provider) => Factory(provider);
}

/// <summary>
///     Lazily initializing <typeparamref name="TInstance"/> instance factory.
/// </summary>
/// <typeparam name="TInstance">Lazily creating type.</typeparam>
/// <typeparam name="TParameter">Parameter type required for creating <typeparamref name="TInstance"/> type.</typeparam>
public class InstanceFactory<TInstance, TParameter> where TInstance : class?
{
    /// <summary>
    ///     Factory method.
    /// </summary>
    protected Func<IServiceProvider, TParameter, TInstance> Factory { get; }

    /// <summary/>
    public InstanceFactory(Func<IServiceProvider, TParameter, TInstance> factory) =>
        this.Factory = factory;

    /// <summary>
    ///     Creates an instance.
    /// </summary>
    public virtual TInstance Create(IServiceProvider provider, TParameter parameter) => Factory(provider, parameter);
}
