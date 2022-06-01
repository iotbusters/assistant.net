using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Regular messaging client builder implementation.
/// </summary>
public class MessagingClientBuilder : MessagingClientBuilder<MessagingClientBuilder>
{
    /// <summary/>
    public MessagingClientBuilder(IServiceCollection services, string name) : base(services, name) { }
}

/// <summary>
///     Basic messaging client builder abstraction.
/// </summary>
/// <typeparam name="TBuilder">Specific messaging client builder implementation type.</typeparam>
public abstract class MessagingClientBuilder<TBuilder> : IMessagingClientBuilder where TBuilder : MessagingClientBuilder<TBuilder>
{
    /// <summary/>
    protected MessagingClientBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public IServiceCollection Services { get; }

    /// <summary/>
    protected TBuilder Builder => (TBuilder)this;

    /// <summary>
    ///     Registers single provider based handler of <typeparamref name="TMessage" />.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled by a single provider.</typeparam>
    /// <exception cref="ArgumentException"/>
    public TBuilder Add<TMessage>() where TMessage : IAbstractMessage =>
        Add(typeof(TMessage));

    /// <summary>
    ///     Registers single provider based handler of <paramref name="messageType" />.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public TBuilder Add(Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        Services.ConfigureMessagingClientOptions(Name, o => o.Add(messageType));
        return Builder;
    }

    /// <summary>
    ///     Registers a local in-memory handler type <typeparamref name="THandler" />.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public TBuilder AddHandler<THandler>()
        where THandler : class => AddHandler(typeof(THandler));

    /// <summary>
    ///     Registers a local in-memory <paramref name="handlerType" />.
    /// </summary>
    /// <param name="handlerType">The message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public TBuilder AddHandler(Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        Services.ConfigureMessagingClientOptions(Name, o => o.AddHandler(handlerType));
        return Builder;
    }

    /// <summary>
    ///     Registers a local in-memory <paramref name="handlerInstance" />.
    /// </summary>
    /// <param name="handlerInstance">The message handler implementation instance.</param>
    /// <exception cref="ArgumentException"/>
    public TBuilder AddHandler(object handlerInstance)
    {
        var handlerType = handlerInstance.GetType();
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

        Services.ConfigureMessagingClientOptions(Name, o => o.AddHandler(handlerInstance));
        return Builder;
    }

    /// <summary>
    ///     Removes all handlers.
    /// </summary>
    public TBuilder ClearHandlers()
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.Handlers.Clear());
        return Builder;
    }

    /// <summary>
    ///     Removes an handler of <typeparamref name="TMessage" />.
    /// </summary>
    /// <typeparam name="TMessage">The message type to find associated handler.</typeparam>
    public TBuilder Remove<TMessage>()
        where TMessage : class => Remove(typeof(TMessage));

    /// <summary>
    ///     Removes an handler of <paramref name="messageType" />.
    /// </summary>
    /// <param name="messageType">The message type to find associated handler.</param>
    public TBuilder Remove(Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        Services.ConfigureMessagingClientOptions(Name, o => o.Handlers.Remove(messageType));
        return Builder;
    }

    /// <summary>
    ///     Removes the handler type <typeparamref name="THandler"/>.
    /// </summary>
    /// <typeparam name="THandler">The message handler type.</typeparam>
    public TBuilder RemoveHandler<THandler>()
        where THandler : class => RemoveHandler(typeof(THandler));

    /// <summary>
    ///     Removes the handler <paramref name="handlerType" />.
    /// </summary>
    /// <param name="handlerType">The message handler implementation type.</param>
    public TBuilder RemoveHandler(Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        Services.ConfigureMessagingClientOptions(Name, o => o.RemoveHandler(handlerType));
        return Builder;
    }

    /// <summary>
    ///     Apply a configuration type <typeparamref name="TConfiguration" />.
    /// </summary>
    public TBuilder AddConfiguration<TConfiguration>()
        where TConfiguration : IMessageConfiguration<TBuilder>, new() => AddConfiguration(new TConfiguration());

    /// <summary>
    ///     Apply a list of configuration instances <paramref name="messageConfigurations" />.
    /// </summary>
    public TBuilder AddConfiguration(params IMessageConfiguration<TBuilder>[] messageConfigurations)
    {
        foreach (var config in messageConfigurations)
            config.Configure(Builder);
        return Builder;
    }

    /// <summary>
    ///     Adds an interceptor type <typeparamref name="TInterceptor" /> .
    /// </summary>
    public TBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : class => AddInterceptor(typeof(TInterceptor));

    /// <summary>
    ///     Adds the interceptor <paramref name="interceptorType" />.
    /// </summary>
    public TBuilder AddInterceptor(Type interceptorType)
    {
        if (!interceptorType.IsMessageInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        Services.ConfigureMessagingClientOptions(Name, o => o.AddInterceptor(interceptorType));
        return Builder;
    }

    /// <summary>
    ///     Adds the interceptor <paramref name="interceptorInstance" />.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public TBuilder AddInterceptor(object interceptorInstance)
    {
        var interceptorType = interceptorInstance.GetType();
        if (!interceptorType.IsMessageInterceptor())
            throw new ArgumentException($"Expected message interceptor but provided {interceptorType}.", nameof(interceptorInstance));

        Services.ConfigureMessagingClientOptions(Name, o => o.AddInterceptor(interceptorInstance));
        return Builder;
    }

    /// <summary>
    ///     Removes all interceptors.
    /// </summary>
    public TBuilder ClearInterceptors()
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.Interceptors.Clear());
        return Builder;
    }

    /// <summary>
    ///     Replaces an interceptor type <typeparamref name="TTargetInterceptor" />
    ///     with <typeparamref name="TReplacementInterceptor"/>.
    /// </summary>
    public TBuilder ReplaceInterceptor<TTargetInterceptor, TReplacementInterceptor>()
        where TTargetInterceptor : class
        where TReplacementInterceptor : class =>
        ReplaceInterceptor(typeof(TTargetInterceptor), typeof(TReplacementInterceptor));

    /// <summary>
    ///     Replace an interceptor type <paramref name="targetType" />
    ///     with <paramref name="replacementType"/>.
    /// </summary>
    public TBuilder ReplaceInterceptor(Type targetType, Type replacementType)
    {
        if (!targetType.IsMessageInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {targetType}.", nameof(targetType));

        Services.ConfigureMessagingClientOptions(Name, o => o.ReplaceInterceptor(targetType, replacementType));
        return Builder;
    }

    /// <summary>
    ///     Removes an interceptor type <typeparamref name="TInterceptor" />.
    /// </summary>
    public TBuilder RemoveInterceptor<TInterceptor>()
        where TInterceptor : class => RemoveInterceptor(typeof(TInterceptor));

    /// <summary>
    ///     Removes an interceptor <paramref name="type" />.
    /// </summary>
    public TBuilder RemoveInterceptor(Type type)
    {
        if (!type.IsMessageInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {type}.", nameof(type));

        Services.ConfigureMessagingClientOptions(Name, o => o.RemoveInterceptor(type));
        return Builder;
    }

    /// <summary>
    ///     Allows exposing the external exception type <typeparamref name="TException" />.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public TBuilder ExposeException<TException>()
        where TException : Exception => ExposeException(typeof(TException));

    /// <summary>
    ///     Allows exposing the exception <paramref name="type"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public TBuilder ExposeException(Type type)
    {
        if (!type.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {type}.", nameof(type));

        Services.ConfigureMessagingClientOptions(Name, o => o.ExposedExceptions.Add(type));
        return Builder;
    }

    /// <summary>
    ///     Removes the exposed exception type <typeparamref name="TException" />.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public TBuilder RemoveExposedException<TException>()
        where TException : Exception
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.ExposedExceptions.Remove(typeof(TException)));
        return Builder;
    }

    /// <summary>
    ///     Removes all exposed external exception types.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public TBuilder ClearExposedExceptions()
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.ExposedExceptions.Clear());
        return Builder;
    }

    /// <summary>
    ///     Allows retrying the transient exception type <typeparamref name="TException" />.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public TBuilder AddTransientException<TException>()
        where TException : Exception => AddTransientException(typeof(TException));

    /// <summary>
    ///     Allows retrying the transient exception <paramref name="type" />.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public TBuilder AddTransientException(Type type)
    {
        if (!type.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {type}.", nameof(type));

        Services.ConfigureMessagingClientOptions(Name, o => o.TransientExceptions.Add(type));
        return Builder;
    }

    /// <summary>
    ///     Removes the transient exception type <typeparamref name="TException" />.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public TBuilder RemoveTransientException<TException>()
        where TException : Exception
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.TransientExceptions.Remove(typeof(TException)));
        return Builder;
    }

    /// <summary>
    ///     Removes all transient exception types.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public TBuilder ClearTransientExceptions()
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.TransientExceptions.Clear());
        return Builder;
    }

    /// <summary>
    ///     Overrides retrying strategy.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public TBuilder Retry(IRetryStrategy strategy)
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.Retry = strategy);
        return Builder;
    }

    /// <summary>
    ///     Overrides retrying strategy.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public TBuilder Retry(IConfigurationSection configuration)
    {
        IRetryStrategy strategy = configuration["type"] switch
        {
            "Exponential" => configuration.Get<ExponentialBackoff>(),
            "Linear" => configuration.Get<LinearBackoff>(),
            "Constant" => configuration.Get<ConstantBackoff>(),
            _ => throw new ArgumentException($"Key 'type' at {configuration.Path} is expected to be: "
                                             + $"'Exponential', 'Linear' or 'Constant' but was '{configuration["type"]}'.")
        };
        return Retry(strategy);
    }

    /// <summary>
    ///     Overrides message handling timeout.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="TimeoutInterceptor"/>.
    /// </remarks>
    public TBuilder TimeoutIn(TimeSpan timeout)
    {
        Services.ConfigureMessagingClientOptions(Name, o => o.Timeout = timeout);
        return Builder;
    }
}
