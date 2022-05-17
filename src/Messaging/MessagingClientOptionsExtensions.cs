using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
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
    ///     Registers a named message handler definition.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddHandler(this MessagingClientOptions options, Type handlerType)
    {
        var handlerInterfaceTypes = handlerType.GetMessageHandlerInterfaceTypes();
        if (!handlerInterfaceTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        var messageTypes = handlerInterfaceTypes.Select(x => x.GetGenericArguments().First());
        foreach (var messageType in messageTypes)
            options.Handlers[messageType] = new HandlerDefinition(p =>
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
        var handlerInterfaceTypes = handlerType.GetMessageHandlerInterfaceTypes();
        if (!handlerInterfaceTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

        var messageTypes = handlerInterfaceTypes.Select(x => x.GetGenericArguments().First());
        foreach (var messageType in messageTypes)
            options.Handlers[messageType] = new HandlerDefinition(p =>
            {
                var providerType = typeof(LocalMessageHandlingProxy<,>).MakeGenericTypeBoundToMessage(messageType);
                var handler = ActivatorUtilities.CreateInstance(p, providerType, handlerInstance);
                return (IAbstractHandler)handler;
            });

        return options;
    }

    /// <summary>
    ///     Removes the <paramref name="handlerType" /> from the list.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions RemoveHandler(this MessagingClientOptions options, Type handlerType)
    {
        var handlerInterfaceTypes = handlerType.GetMessageHandlerInterfaceTypes();
        if (!handlerInterfaceTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        var messageTypes = handlerInterfaceTypes.Select(x => x.GetGenericArguments().First());
        foreach (var messageType in messageTypes)
            options.Handlers.Remove(messageType);

        return options;
    }

    /// <summary>
    ///     Adds the <paramref name="interceptorType" /> to the end of the list.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddInterceptor(this MessagingClientOptions options, Type interceptorType)
    {
        if (!interceptorType.IsMessageInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        var messageTypes = interceptorType.GetMessageInterceptorInterfaceTypes().Select(x => x.GetGenericArguments().First());
        foreach (var messageType in messageTypes)
            options.Interceptors.Add(new InterceptorDefinition(messageType, interceptorType, p =>
            {
                var interceptor = interceptorType.GetConstructors()
                    .Any(x => x.GetParameters().Any(y => y.ParameterType == typeof(MessagingClientOptions)))
                    ? ActivatorUtilities.CreateInstance(p, interceptorType, options)
                    : ActivatorUtilities.CreateInstance(p, interceptorType);
                var wrapper = ActivatorUtilities.CreateInstance(
                    p,
                    typeof(AbstractInterceptor<,,>).MakeGenericType(interceptorType, messageType, messageType.GetResponseType()!),
                    interceptor);
                return (IAbstractInterceptor)wrapper;
            }));

        return options;
    }

    /// <summary>
    ///     Adds the <paramref name="interceptorInstance" /> to the end of the list.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddInterceptor(this MessagingClientOptions options, object interceptorInstance)
    {
        var interceptorType = interceptorInstance.GetType();
        if (!interceptorType.IsMessageInterceptor())
            throw new ArgumentException($"Expected message interceptor but provided {interceptorType}.", nameof(interceptorInstance));

        var messageTypes = interceptorType.GetMessageInterceptorInterfaceTypes().Select(x => x.GetGenericArguments().First());
        foreach (var messageType in messageTypes)
            options.Interceptors.Add(new InterceptorDefinition(messageType, interceptorInstance.GetType(), p =>
            {
                var wrapper = ActivatorUtilities.CreateInstance(
                    p,
                    typeof(AbstractInterceptor<,,>).MakeGenericType(interceptorType, messageType, messageType.GetResponseType()!),
                    interceptorInstance);
                return (IAbstractInterceptor)wrapper;
            }));

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
            options.Interceptors.Insert(index, new InterceptorDefinition(definition.MessageType, replacementType, p =>
            {
                var interceptor = replacementType.GetConstructors()
                    .Any(x => x.GetParameters().Any(y => y.ParameterType == typeof(MessagingClientOptions)))
                    ? ActivatorUtilities.CreateInstance(p, replacementType, options)
                    : ActivatorUtilities.CreateInstance(p, replacementType);
                var wrapper = ActivatorUtilities.CreateInstance(
                    p,
                    typeof(AbstractInterceptor<,,>).MakeGenericType(replacementType, definition.MessageType, definition.MessageType.GetResponseType()!),
                    interceptor);
                return (IAbstractInterceptor)wrapper;
            }));
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