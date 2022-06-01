using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures the messaging client to use a single provider feature.
    /// </summary>
    public static MessagingClientOptions UseSingleProvider(this MessagingClientOptions options, Func<IServiceProvider, IAbstractHandler> factory)
    {
        options.SingleProvider = new(factory);
        return options;
    }

    /// <summary>
    ///     Registers single provider based handler of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled by a single provider.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions Add<TMessage>(this MessagingClientOptions options)
        where TMessage : IAbstractMessage => options
        .Add(typeof(TMessage));

    /// <summary>
    ///     Registers single provider based handler of <paramref name="messageType" />.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions Add(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.Handlers[messageType] = new(p =>
        {
            var definition = options.SingleProvider ?? throw new ArgumentException("Single provider wasn't properly configured.");
            return definition.Create(p);
        });

        return options;
    }

    /// <summary>
    ///     Registers a named message handler definition.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddHandler(this MessagingClientOptions options, Type handlerType)
    {
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        foreach (var messageType in messageTypes)
            options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            {
                var handlerInstance = ActivatorUtilities.CreateInstance(p, handlerType);
                var providerType = typeof(LocalMessageHandlingProxy<,>).MakeGenericTypeBoundToMessage(messageType);
                var provider = ActivatorUtilities.CreateInstance(p, providerType, handlerInstance);
                return (IAbstractHandler)provider;
            });

        return options;
    }

    /// <summary>
    ///     Registers a named message handler definition.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddHandler(this MessagingClientOptions options, object handlerInstance)
    {
        var handlerType = handlerInstance.GetType();
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

        foreach (var messageType in messageTypes)
            options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            {
                var providerType = typeof(LocalMessageHandlingProxy<,>).MakeGenericTypeBoundToMessage(messageType);
                var handler = ActivatorUtilities.CreateInstance(p, providerType, handlerInstance);
                return (IAbstractHandler)handler;
            });

        return options;
    }

    /// <summary>
    ///     Removes the <paramref name="handlerType"/> from the list.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions RemoveHandler(this MessagingClientOptions options, Type handlerType)
    {
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        foreach (var messageType in messageTypes)
            options.Handlers.Remove(messageType);

        return options;
    }

    /// <summary>
    ///     Adds the <paramref name="interceptorType"/> to the end of the list.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddInterceptor(this MessagingClientOptions options, Type interceptorType)
    {
        var messageTypes = interceptorType.GetMessageInterceptorInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if(!messageTypes.Any())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        foreach (var messageType in messageTypes)
        {
            var factory = new InstanceCachingFactory<IAbstractInterceptor>(p =>
            {
                var interceptor = ActivatorUtilities.CreateInstance(p, interceptorType);
                var responseType = messageType.GetResponseType();
                var abstractInterceptorType = typeof(AbstractInterceptor<,,>).MakeGenericType(interceptorType, messageType, responseType!);
                var abstractInterceptor = ActivatorUtilities.CreateInstance(p, abstractInterceptorType, interceptor);
                return (IAbstractInterceptor)abstractInterceptor;
            });
            options.Interceptors.Add(new InterceptorDefinition(messageType, interceptorType, factory));
        }

        return options;
    }

    /// <summary>
    ///     Adds the <paramref name="interceptorInstance"/> to the end of the list.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddInterceptor(this MessagingClientOptions options, object interceptorInstance)
    {
        var interceptorType = interceptorInstance.GetType();
        var messageTypes = interceptorType.GetMessageInterceptorInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message interceptor but provided {interceptorType}.", nameof(interceptorInstance));

        var factory = new InstanceFactory<IAbstractInterceptor>(_ => (IAbstractInterceptor)interceptorInstance);
        foreach (var messageType in messageTypes)
            options.Interceptors.Add(new InterceptorDefinition(messageType, interceptorType, factory));

        return options;
    }

    /// <summary>
    ///     Replaces matching messages of the interceptor type <paramref name="targetType" /> in the list with <paramref name="replacementType"/>.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions ReplaceInterceptor(this MessagingClientOptions options, Type targetType, Type replacementType)
    {
        if (!replacementType.IsMessageInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {replacementType}.", nameof(replacementType));

        var messageTypes = replacementType.GetMessageInterceptorInterfaceTypes().Select(x => x.GetGenericArguments().First());
        var definitions = options.Interceptors
            .Where(x => x.InterceptorType == targetType && messageTypes.Contains(x.MessageType))
            .ToArray();

        if (!definitions.Any())
            return options.AddInterceptor(replacementType);

        foreach (var definition in definitions)
        {
            var index = options.Interceptors.IndexOf(definition);
            options.Interceptors.RemoveAt(index);
            var factory = new InstanceCachingFactory<IAbstractInterceptor>(p =>
            {
                var interceptor = p.GetService(replacementType) ?? ActivatorUtilities.CreateInstance(p, replacementType);
                return (IAbstractInterceptor)interceptor;
            });
            options.Interceptors.Insert(index, new InterceptorDefinition(definition.MessageType, replacementType, factory));
        }

        return options;
    }

    /// <summary>
    ///     Removes the interceptor type <paramref name="interceptorType" /> from the list.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions RemoveInterceptor(this MessagingClientOptions options, Type interceptorType)
    {
        if (!interceptorType.IsMessageInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        var definitions = options.Interceptors.Where(x => x.InterceptorType == interceptorType).ToArray();
        foreach (var definition in definitions)
            options.Interceptors.Remove(definition);

        return options;
    }

    /// <summary>
    ///     Removes all interceptors from the list.
    /// </summary>
    public static MessagingClientOptions ClearInterceptors(this MessagingClientOptions options)
    {
        options.Interceptors.Clear();
        return options;
    }
}
