using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client configuration extensions.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Registers single provider based handler of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled by a single provider.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddSingle<TMessage>(this MessagingClientBuilder builder) where TMessage : IAbstractMessage => builder
        .AddSingle(typeof(TMessage));

    /// <summary>
    ///     Registers single provider based handler of <paramref name="messageType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <param name="builder"/>
    /// <param name="messageType">The message type to find associated handler.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddSingle(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddSingle(messageType));
        return builder;
    }

    /// <summary>
    ///     Registers single provider based handler of any message type except defined explicitly.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <param name="builder"/>
    public static MessagingClientBuilder AddSingleAny(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddSingleAny());
        return builder;
    }

    /// <summary>
    ///     Registers a local in-memory handler type <typeparamref name="THandler"/>.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddHandler<THandler>(this MessagingClientBuilder builder)
        where THandler : class => builder.AddHandler(typeof(THandler));

    /// <summary>
    ///     Registers a local in-memory <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddHandler(this MessagingClientBuilder builder, Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Registers a local in-memory <paramref name="handlerInstance"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerInstance">The message handler implementation instance.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddHandler(this MessagingClientBuilder builder, object handlerInstance)
    {
        var handlerType = handlerInstance.GetType();
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddHandler(handlerInstance));
        return builder;
    }

    /// <summary>
    ///     Removes all handlers.
    /// </summary>
    public static MessagingClientBuilder ClearHandlers(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Handlers.Clear());
        return builder;
    }

    /// <summary>
    ///     Removes an handler of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The message type to find associated handler.</typeparam>
    public static MessagingClientBuilder Remove<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class => builder.Remove(typeof(TMessage));

    /// <summary>
    ///     Removes an handler of <paramref name="messageType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="messageType">The message type to find associated handler.</param>
    public static MessagingClientBuilder Remove(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Handlers.Remove(messageType));
        return builder;
    }

    /// <summary>
    ///     Removes the handler type <typeparamref name="THandler"/>.
    /// </summary>
    /// <typeparam name="THandler">The message handler type.</typeparam>
    public static MessagingClientBuilder RemoveHandler<THandler>(this MessagingClientBuilder builder)
        where THandler : class => builder.RemoveHandler(typeof(THandler));

    /// <summary>
    ///     Removes the handler <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    public static MessagingClientBuilder RemoveHandler(this MessagingClientBuilder builder, Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.RemoveHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Apply a configuration type <typeparamref name="TConfiguration"/>.
    /// </summary>
    public static MessagingClientBuilder AddConfiguration<TConfiguration>(this MessagingClientBuilder builder)
        where TConfiguration : IMessageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

    /// <summary>
    ///     Apply a list of configuration instances <paramref name="messageConfigurations"/>.
    /// </summary>
    public static MessagingClientBuilder AddConfiguration(this MessagingClientBuilder builder, params IMessageConfiguration[] messageConfigurations)
    {
        foreach (var config in messageConfigurations)
            config.Configure(builder);
        return builder;
    }

    /// <summary>
    ///     Adds an interceptor type <typeparamref name="TInterceptor"/> .
    /// </summary>
    public static MessagingClientBuilder AddInterceptor<TInterceptor>(this MessagingClientBuilder builder)
        where TInterceptor : class => builder.AddInterceptor(typeof(TInterceptor));

    /// <summary>
    ///     Adds the interceptor <paramref name="interceptorType"/>.
    /// </summary>
    public static MessagingClientBuilder AddInterceptor(this MessagingClientBuilder builder, Type interceptorType)
    {
        if (!interceptorType.IsRequestMessageInterceptor() && !interceptorType.IsAbstractRequestInterceptor()
            && !interceptorType.IsPublishMessageInterceptor() && !interceptorType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddInterceptor(interceptorType));
        return builder;
    }

    /// <summary>
    ///     Adds the interceptor <paramref name="interceptorInstance"/>.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddInterceptor(this MessagingClientBuilder builder, object interceptorInstance)
    {
        var interceptorType = interceptorInstance.GetType();
        if (!interceptorType.IsRequestMessageInterceptor() && !interceptorType.IsAbstractRequestInterceptor()
            && !interceptorType.IsPublishMessageInterceptor() && !interceptorType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected message interceptor but provided {interceptorType}.", nameof(interceptorInstance));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddInterceptor(interceptorInstance));
        return builder;
    }

    /// <summary>
    ///     Removes all interceptors.
    /// </summary>
    public static MessagingClientBuilder ClearInterceptors(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ClearInterceptors());
        return builder;
    }

    /// <summary>
    ///     Replaces an interceptor type <typeparamref name="TTargetInterceptor"/>
    ///     with <typeparamref name="TReplacementInterceptor"/>.
    /// </summary>
    public static MessagingClientBuilder ReplaceInterceptor<TTargetInterceptor, TReplacementInterceptor>(this MessagingClientBuilder builder)
        where TTargetInterceptor : class
        where TReplacementInterceptor : class => builder
        .ReplaceInterceptor(typeof(TTargetInterceptor), typeof(TReplacementInterceptor));

    /// <summary>
    ///     Replace an interceptor type <paramref name="targetType"/>
    ///     with <paramref name="replacementType"/>.
    /// </summary>
    public static MessagingClientBuilder ReplaceInterceptor(this MessagingClientBuilder builder, Type targetType, Type replacementType)
    {
        if (!replacementType.IsRequestMessageInterceptor() && !replacementType.IsAbstractRequestInterceptor()
            && !replacementType.IsPublishMessageInterceptor() && !replacementType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {targetType}.", nameof(targetType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ReplaceInterceptor(targetType, replacementType));
        return builder;
    }

    /// <summary>
    ///     Removes an interceptor type <typeparamref name="TInterceptor"/>.
    /// </summary>
    public static MessagingClientBuilder RemoveInterceptor<TInterceptor>(this MessagingClientBuilder builder)
        where TInterceptor : class => builder.RemoveInterceptor(typeof(TInterceptor));

    /// <summary>
    ///     Removes an interceptor <paramref name="interceptorType"/>.
    /// </summary>
    public static MessagingClientBuilder RemoveInterceptor(this MessagingClientBuilder builder, Type interceptorType)
    {
        if (!interceptorType.IsRequestMessageInterceptor() && !interceptorType.IsAbstractRequestInterceptor()
            && !interceptorType.IsPublishMessageInterceptor() && !interceptorType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.RemoveInterceptor(interceptorType));
        return builder;
    }

    /// <summary>
    ///     Allows exposing the external exception type <typeparamref name="TException"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder ExposeException<TException>(this MessagingClientBuilder builder)
        where TException : Exception => builder.ExposeException(typeof(TException));

    /// <summary>
    ///     Allows exposing the exception <paramref name="type"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder ExposeException(this MessagingClientBuilder builder, Type type)
    {
        if (!type.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {type}.", nameof(type));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ExposedExceptions.Add(type));
        return builder;
    }

    /// <summary>
    ///     Removes the exposed exception type <typeparamref name="TException"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder RemoveExposedException<TException>(this MessagingClientBuilder builder)
        where TException : Exception
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ExposedExceptions.Remove(typeof(TException)));
        return builder;
    }

    /// <summary>
    ///     Removes all exposed external exception types.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder ClearExposedExceptions(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ExposedExceptions.Clear());
        return builder;
    }

    /// <summary>
    ///     Allows retrying the transient exception type <typeparamref name="TException"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder AddTransientException<TException>(this MessagingClientBuilder builder)
        where TException : Exception => builder.AddTransientException(typeof(TException));

    /// <summary>
    ///     Allows retrying the transient exception <paramref name="type"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder AddTransientException(this MessagingClientBuilder builder, Type type)
    {
        if (!type.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {type}.", nameof(type));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.TransientExceptions.Add(type));
        return builder;
    }

    /// <summary>
    ///     Removes the transient exception type <typeparamref name="TException"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder RemoveTransientException<TException>(this MessagingClientBuilder builder)
        where TException : Exception
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.TransientExceptions.Remove(typeof(TException)));
        return builder;
    }

    /// <summary>
    ///     Removes all transient exception types.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder ClearTransientExceptions(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.TransientExceptions.Clear());
        return builder;
    }

    /// <summary>
    ///     Overrides retrying strategy.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder Retry(this MessagingClientBuilder builder, IRetryStrategy strategy)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Retry = strategy);
        return builder;
    }

    /// <summary>
    ///     Overrides retrying strategy.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder Retry(this MessagingClientBuilder builder, IConfigurationSection configuration)
    {
        IRetryStrategy strategy = configuration["type"] switch
        {
            "Exponential" => configuration.Get<ExponentialBackoff>()!,
            "Linear" => configuration.Get<LinearBackoff>()!,
            "Constant" => configuration.Get<ConstantBackoff>()!,
            _ => throw new ArgumentException($"Key 'type' at {configuration.Path} is expected to be: "
                                             + $"'Exponential', 'Linear' or 'Constant' but was '{configuration["type"]}'.")
        };
        return builder.Retry(strategy);
    }

    /// <summary>
    ///     Overrides message handling timeout.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="TimeoutInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder TimeoutIn(this MessagingClientBuilder builder, TimeSpan timeout)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Timeout = timeout);
        return builder;
    }
}
