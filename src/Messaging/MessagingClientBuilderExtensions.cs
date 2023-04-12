using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client configuration extensions.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures a local storage for message handlers and interceptors.
    /// </summary>
    /// <remarks>
    ///     This is just an alias to <see cref="StorageBuilderExtensions.UseLocal"/> and
    ///     required only for some interceptors.
    /// </remarks>
    public static MessagingClientBuilder UseLocal(this MessagingClientBuilder builder)
    {
        builder.Services.AddStorage(builder.Name, b => b.UseLocal());
        return builder;
    }

    /// <summary>
    ///     Registers a <typeparamref name="TMessage"/> type single handler.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleHandler methods;
    ///     the method overrides already registered <typeparamref name="TMessage"/> type handler.
    /// </remarks>
    /// <typeparam name="TMessage">A message type to be handled by a single handler.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddSingle<TMessage>(this MessagingClientBuilder builder) where TMessage : IAbstractMessage => builder
        .AddSingle(typeof(TMessage));

    /// <summary>
    ///     Registers a <paramref name="messageType"/> single handler.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method;
    ///     the method overrides already registered <paramref name="messageType"/> handler.
    /// </remarks>
    /// <param name="builder"/>
    /// <param name="messageType">A message type to find associated handler.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddSingle(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddSingle(messageType));
        return builder;
    }

    /// <summary>
    ///     Registers a in-memory <typeparamref name="THandler"/> type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered message types.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddHandler<THandler>(this MessagingClientBuilder builder)
        where THandler : class => builder.AddHandler(typeof(THandler));

    /// <summary>
    ///     Registers a in-memory <paramref name="handlerType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered message types.
    /// </remarks>
    /// <param name="builder"/>
    /// <param name="handlerType">A message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddHandler(this MessagingClientBuilder builder, Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Registers a in-memory <paramref name="handlerInstance"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered message types.
    /// </remarks>
    /// <param name="builder"/>
    /// <param name="handlerInstance">A message handler implementation instance.</param>
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
    ///     Removes all registered message handlers.
    /// </summary>
    public static MessagingClientBuilder ClearHandlers(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.HandlerFactories.Clear());
        return builder;
    }

    /// <summary>
    ///     Removes a <typeparamref name="TMessage"/> type handler.
    /// </summary>
    /// <typeparam name="TMessage">A message type to find associated handler.</typeparam>
    public static MessagingClientBuilder Remove<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class => builder.Remove(typeof(TMessage));

    /// <summary>
    ///     Removes a <paramref name="messageType"/> handler.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="messageType">A message type to find associated handler.</param>
    public static MessagingClientBuilder Remove(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.HandlerFactories.Remove(messageType));
        return builder;
    }

    /// <summary>
    ///     Removes a message <typeparamref name="THandler"/> type.
    /// </summary>
    /// <typeparam name="THandler">A message handler type.</typeparam>
    public static MessagingClientBuilder RemoveHandler<THandler>(this MessagingClientBuilder builder)
        where THandler : class => builder.RemoveHandler(typeof(THandler));

    /// <summary>
    ///     Removes a message <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerType">A message handler implementation type.</param>
    public static MessagingClientBuilder RemoveHandler(this MessagingClientBuilder builder, Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.RemoveHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Applies a message <typeparamref name="TConfiguration"/> type.
    /// </summary>
    public static MessagingClientBuilder AddConfiguration<TConfiguration>(this MessagingClientBuilder builder)
        where TConfiguration : IMessageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

    /// <summary>
    ///     Applies message <paramref name="configurationInstances"/>.
    /// </summary>
    public static MessagingClientBuilder AddConfiguration(this MessagingClientBuilder builder, params IMessageConfiguration[] configurationInstances)
    {
        foreach (var config in configurationInstances)
            config.Configure(builder);
        return builder;
    }

    /// <summary>
    ///     Adds an interceptor type <typeparamref name="TInterceptor"/> .
    /// </summary>
    public static MessagingClientBuilder AddInterceptor<TInterceptor>(this MessagingClientBuilder builder)
        where TInterceptor : class => builder.AddInterceptor(typeof(TInterceptor));

    /// <summary>
    ///     Adds a message <paramref name="interceptorType"/>.
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
    ///     Adds a message <paramref name="interceptorInstance"/>.
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
    ///     Removes all message interceptors.
    /// </summary>
    public static MessagingClientBuilder ClearInterceptors(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ClearInterceptors());
        return builder;
    }

    /// <summary>
    ///     Replaces a message <typeparamref name="TTargetInterceptor"/> type with <typeparamref name="TReplacementInterceptor"/> type.
    /// </summary>
    public static MessagingClientBuilder ReplaceInterceptor<TTargetInterceptor, TReplacementInterceptor>(this MessagingClientBuilder builder)
        where TTargetInterceptor : class
        where TReplacementInterceptor : class => builder
        .ReplaceInterceptor(typeof(TTargetInterceptor), typeof(TReplacementInterceptor));

    /// <summary>
    ///     Replace a message interceptor <paramref name="targetType"/> with <paramref name="replacementType"/>.
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
    ///     Removes a message <typeparamref name="TInterceptor"/> type.
    /// </summary>
    public static MessagingClientBuilder RemoveInterceptor<TInterceptor>(this MessagingClientBuilder builder)
        where TInterceptor : class => builder.RemoveInterceptor(typeof(TInterceptor));

    /// <summary>
    ///     Removes a message <paramref name="interceptorType"/>.
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
    ///     Allows exposing an external <typeparamref name="TException"/> type.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder ExposeException<TException>(this MessagingClientBuilder builder)
        where TException : Exception => builder.ExposeException(typeof(TException));

    /// <summary>
    ///     Allows exposing an external <paramref name="exceptionType"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder ExposeException(this MessagingClientBuilder builder, Type exceptionType)
    {
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ExposedExceptions.Add(exceptionType));
        return builder;
    }

    /// <summary>
    ///     Removes an exposed <typeparamref name="TException"/> type.
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
    ///     Allows retrying a transient <typeparamref name="TException"/> type.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder AddTransientException<TException>(this MessagingClientBuilder builder)
        where TException : Exception => builder.AddTransientException(typeof(TException));

    /// <summary>
    ///     Allows retrying a transient <paramref name="exceptionType"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientBuilder AddTransientException(this MessagingClientBuilder builder, Type exceptionType)
    {
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.TransientExceptions.Add(exceptionType));
        return builder;
    }

    /// <summary>
    ///     Removes a transient exception type <typeparamref name="TException"/>.
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
    ///     Overrides the retrying strategy.
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
    ///     Overrides the retrying strategy.
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
    ///     Overrides the message handling timeout.
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
