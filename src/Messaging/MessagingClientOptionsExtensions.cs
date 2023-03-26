using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using System;
using System.Collections.Generic;
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
    /// <param name="options"/>
    /// <param name="factory">Remote single provider factory.</param>
    /// <remarks>
    ///     The provider accepts all message types registered by calling one of <see cref="AddSingle"/> extension methods.
    /// </remarks>
    public static MessagingClientOptions UseSingleProvider(this MessagingClientOptions options, Func<IServiceProvider, IAbstractHandler> factory)
    {
        options.SingleProvider = new InstanceCachingFactory<IAbstractHandler>(factory);
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
    public static MessagingClientOptions AddSingle<TMessage>(this MessagingClientOptions options)
        where TMessage : IAbstractMessage => options
        .AddSingle(typeof(TMessage));

    /// <summary>
    ///     Registers single provider based handler of <paramref name="messageType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="messageType">Accepting message type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddSingle(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
        {
            var definition = options.SingleProvider ?? throw new ArgumentException("Single provider wasn't properly configured.");
            return definition.Create(p);
        });

        return options;
    }

    /// <summary>
    ///     Registers single provider based handler of any message type except defined explicitly.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of Use***SingleProvider method.
    /// </remarks>
    public static MessagingClientOptions AddSingleAny(this MessagingClientOptions options) => options
        .AddDefaultHandler(p =>
        {
            var definition = options.SingleProvider ?? throw new ArgumentException("Single provider wasn't properly configured.");
            return definition.Create(p);
        });

    /// <summary>
    ///     Registers a handler of any message type if none defined explicitly.
    /// </summary>
    /// <param name="options"/>
    /// <param name="defaultHandler">A message handler instance.</param>
    public static MessagingClientOptions AddDefaultHandler(this MessagingClientOptions options, IAbstractHandler defaultHandler) => options
        .AddDefaultHandler(_ => defaultHandler);

    /// <summary>
    ///     Registers a handler of any message type if none defined explicitly.
    /// </summary>
    /// <param name="options"/>
    /// <param name="factory">A message handler factory.</param>
    public static MessagingClientOptions AddDefaultHandler(this MessagingClientOptions options, Func<IServiceProvider, IAbstractHandler> factory)
    {
        options.DefaultHandler = new InstanceCachingFactory<IAbstractHandler>(factory);
        return options;
    }

    /// <summary>
    ///     Registers a named message handler definition.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="handlerType">Message handler type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddHandler(this MessagingClientOptions options, Type handlerType)
    {
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        foreach (var messageType in messageTypes)
            options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            {
                var handlerInstance = p.Create(handlerType);
                var providerType = typeof(LocalMessageHandlingProxy<,>).MakeGenericTypeBoundToMessage(messageType);
                var provider = p.Create(providerType, handlerInstance);
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
    /// <param name="options"/>
    /// <param name="handlerInstance">Message handler instance.</param>
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
                var handler = p.Create(providerType, handlerInstance);
                return (IAbstractHandler)handler;
            });

        return options;
    }

    /// <summary>
    ///     Removes the <paramref name="handlerType"/> from the list.
    /// </summary>
    /// <param name="options"/>
    /// <param name="handlerType">Message handler type.</param>
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
    /// <param name="options"/>
    /// <param name="interceptorType">Message interceptor type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddInterceptor(this MessagingClientOptions options, Type interceptorType)
    {
        if (!interceptorType.IsRequestMessageInterceptor() && !interceptorType.IsAbstractRequestInterceptor()
            && !interceptorType.IsPublishMessageInterceptor() && !interceptorType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        options.RequestInterceptors.AddAbstractInterceptor(interceptorType);
        options.PublishInterceptors.AddAbstractInterceptor(interceptorType);

        return options;
    }

    /// <summary>
    ///     Adds the <paramref name="interceptorInstance"/> to the end of the list.
    /// </summary>
    /// <param name="options"/>
    /// <param name="interceptorInstance">Message interceptor instance.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddInterceptor(this MessagingClientOptions options, object interceptorInstance)
    {
        var interceptorType = interceptorInstance.GetType();
        if (!interceptorType.IsRequestMessageInterceptor() && !interceptorType.IsAbstractRequestInterceptor()
            && !interceptorType.IsPublishMessageInterceptor() && !interceptorType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorInstance));

        options.RequestInterceptors.AddAbstractInterceptor(interceptorInstance);
        options.PublishInterceptors.AddAbstractInterceptor(interceptorInstance);

        return options;
    }

    /// <summary>
    ///     Replaces matching messages of the interceptor type <paramref name="targetType"/>
    ///     in the list with <paramref name="replacementType"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="targetType">Message interceptor type to be replaced.</param>
    /// <param name="replacementType">Message interceptor type to be used instead.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions ReplaceInterceptor(this MessagingClientOptions options, Type targetType, Type replacementType)
    {
        if (!replacementType.IsRequestMessageInterceptor() && !replacementType.IsAbstractRequestInterceptor()
            && !replacementType.IsPublishMessageInterceptor() && !replacementType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {replacementType}.", nameof(replacementType));

        options.RequestInterceptors.ReplaceAbstractInterceptor(targetType, replacementType);
        options.PublishInterceptors.ReplaceAbstractInterceptor(targetType, replacementType);

        return options;
    }

    /// <summary>
    ///     Replaces matching messages of the interceptor type <paramref name="targetType"/>
    ///     in the list with <paramref name="replacementInstance"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="targetType">Message interceptor type to be replaced.</param>
    /// <param name="replacementInstance">Message interceptor instance to be used instead.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions ReplaceInterceptor(this MessagingClientOptions options, Type targetType, object replacementInstance)
    {
        var  interceptorType = replacementInstance.GetType();
        if (! interceptorType.IsRequestMessageInterceptor() && ! interceptorType.IsAbstractRequestInterceptor()
            && ! interceptorType.IsPublishMessageInterceptor() && ! interceptorType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided { interceptorType}.", nameof(replacementInstance));

        options.RequestInterceptors.ReplaceAbstractInterceptor(targetType, replacementInstance);
        options.PublishInterceptors.ReplaceAbstractInterceptor(targetType, replacementInstance);

        return options;
    }

    /// <summary>
    ///     Removes the interceptor type <paramref name="interceptorType"/> from the list.
    /// </summary>
    /// <param name="options"/>
    /// <param name="interceptorType">Message interceptor type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions RemoveInterceptor(this MessagingClientOptions options, Type interceptorType)
    {
        if (!interceptorType.IsRequestMessageInterceptor() && !interceptorType.IsAbstractRequestInterceptor()
            && !interceptorType.IsPublishMessageInterceptor() && !interceptorType.IsAbstractPublishInterceptor())
            throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

        var requestDefinitions = options.RequestInterceptors.Where(x => x.InterceptorType == interceptorType).ToArray();
        foreach (var definition in requestDefinitions)
            options.RequestInterceptors.Remove(definition);

        var publishDefinitions = options.PublishInterceptors.Where(x => x.InterceptorType == interceptorType).ToArray();
        foreach (var definition in publishDefinitions)
            options.PublishInterceptors.Remove(definition);

        return options;
    }

    /// <summary>
    ///     Removes all interceptors from the list.
    /// </summary>
    public static MessagingClientOptions ClearInterceptors(this MessagingClientOptions options)
    {
        options.RequestInterceptors.Clear();
        options.PublishInterceptors.Clear();
        return options;
    }

    private static void AddAbstractInterceptor(this IList<InterceptorDefinition<IAbstractRequestInterceptor>> list, Type interceptorType)
    {
        var messageTypes = list.GetUndefinedMessageTypes(interceptorType);
        foreach (var messageType in messageTypes)
        {
            var factory = new InstanceCachingFactory<IAbstractRequestInterceptor>(p =>
                CreateAbstractRequestInterceptor(p, messageType, p.Create(interceptorType)));
            list.Add(new InterceptorDefinition<IAbstractRequestInterceptor>(messageType, interceptorType, factory));
        }
    }

    private static void AddAbstractInterceptor(this IList<InterceptorDefinition<IAbstractPublishInterceptor>> list, Type interceptorType)
    {
        var newMessageTypes = list.GetUndefinedMessageTypes(interceptorType);
        foreach (var messageType in newMessageTypes)
        {
            var factory = new InstanceCachingFactory<IAbstractPublishInterceptor>(p =>
                CreateAbstractPublishInterceptor(p, messageType, p.Create(interceptorType)));
            list.Add(new InterceptorDefinition<IAbstractPublishInterceptor>(messageType, interceptorType, factory));
        }
    }

    private static void AddAbstractInterceptor(this IList<InterceptorDefinition<IAbstractRequestInterceptor>> list, object interceptorInstance)
    {
        var interceptorType = interceptorInstance.GetType();
        var messageTypes = list.GetUndefinedMessageTypes(interceptorType);
        foreach (var messageType in messageTypes)
        {
            var factory = new InstanceCachingFactory<IAbstractRequestInterceptor>(p =>
                CreateAbstractRequestInterceptor(p, messageType, interceptorInstance));
            list.Add(new InterceptorDefinition<IAbstractRequestInterceptor>(messageType, interceptorType, factory));
        }
    }

    private static void AddAbstractInterceptor(this IList<InterceptorDefinition<IAbstractPublishInterceptor>> list, object interceptorInstance)
    {
        var interceptorType = interceptorInstance.GetType();
        var newMessageTypes = list.GetUndefinedMessageTypes(interceptorType);
        foreach (var messageType in newMessageTypes)
        {
            var factory = new InstanceCachingFactory<IAbstractPublishInterceptor>(p =>
                CreateAbstractPublishInterceptor(p, messageType, interceptorInstance));
            list.Add(new InterceptorDefinition<IAbstractPublishInterceptor>(messageType, interceptorType, factory));
        }
    }

    private static void ReplaceAbstractInterceptor(this IList<InterceptorDefinition<IAbstractRequestInterceptor>> list, Type targetType, Type replacementType)
    {
        var messageTypes = GetRequestMessageTypes(replacementType);
        var definitions = list.Where(x => x.InterceptorType == targetType && messageTypes.Contains(x.MessageType)).ToArray();
        foreach (var definition in definitions)
            list.ReplaceDefinition(definition, replacementType);
    }

    private static void ReplaceAbstractInterceptor(this IList<InterceptorDefinition<IAbstractPublishInterceptor>> list, Type targetType, Type replacementType)
    {
        var messageTypes = GetPublishMessageTypes(replacementType);
        var definitions = list.Where(x => x.InterceptorType == targetType && messageTypes.Contains(x.MessageType)).ToArray();
        foreach (var definition in definitions)
            list.ReplaceDefinition(definition, replacementType);
    }

    private static void ReplaceAbstractInterceptor(this IList<InterceptorDefinition<IAbstractRequestInterceptor>> list, Type targetType, object replacementInstance)
    {
        var interceptorType = replacementInstance.GetType();
        var messageTypes = GetRequestMessageTypes(interceptorType);
        var definitions = list.Where(x => x.InterceptorType == targetType && messageTypes.Contains(x.MessageType)).ToArray();
        foreach (var definition in definitions)
            list.ReplaceDefinition(definition, replacementInstance);
    }

    private static void ReplaceAbstractInterceptor(this IList<InterceptorDefinition<IAbstractPublishInterceptor>> list, Type targetType, object replacementInstance)
    {
        var interceptorType = replacementInstance.GetType();
        var messageTypes = GetPublishMessageTypes(interceptorType);
        var definitions = list.Where(x => x.InterceptorType == targetType && messageTypes.Contains(x.MessageType)).ToArray();
        foreach (var definition in definitions)
            list.ReplaceDefinition(definition, replacementInstance);
    }

    private static IEnumerable<Type> GetUndefinedMessageTypes(this IList<InterceptorDefinition<IAbstractRequestInterceptor>> list, Type interceptorType) =>
        GetRequestMessageTypes(interceptorType).Where(x => !list.Any(d => d.MessageType == x && d.InterceptorType == interceptorType));

    private static IEnumerable<Type> GetUndefinedMessageTypes(this IList<InterceptorDefinition<IAbstractPublishInterceptor>> list, Type interceptorType) =>
        GetPublishMessageTypes(interceptorType).Where(x => !list.Any(d => d.MessageType == x && d.InterceptorType == interceptorType));

    private static IEnumerable<Type> GetRequestMessageTypes(Type interceptorType)
    {
        var messageTypes = interceptorType.GetMessageRequestInterceptorInterfaceTypes().Select(x => x.GetGenericArguments().First());
        if (interceptorType.IsAbstractRequestInterceptor())
            return messageTypes.Append(typeof(object));
        return messageTypes;
    }

    private static IEnumerable<Type> GetPublishMessageTypes(Type interceptorType)
    {
        var messageTypes = interceptorType.GetMessagePublishInterceptorInterfaceTypes().Select(x => x.GetGenericArguments().First());
        if (interceptorType.IsAbstractPublishInterceptor())
            return messageTypes.Append(typeof(object));
        return messageTypes;
    }

    private static IAbstractRequestInterceptor CreateAbstractRequestInterceptor(IServiceProvider provider, Type messageType, object interceptorInstance)
    {
        if (interceptorInstance is IAbstractRequestInterceptor interceptor)
            return interceptor;

        var responseType = messageType.GetResponseType()!;
        var abstractInterceptorType = typeof(AbstractRequestInterceptor<,>).MakeGenericType(messageType, responseType);
        var abstractInterceptor = provider.Create(abstractInterceptorType, interceptorInstance);
        return (IAbstractRequestInterceptor)abstractInterceptor;
    }

    private static IAbstractPublishInterceptor CreateAbstractPublishInterceptor(IServiceProvider provider, Type messageType, object interceptorInstance)
    {
        if (interceptorInstance is IAbstractPublishInterceptor interceptor)
            return interceptor;

        var abstractInterceptorType = typeof(AbstractPublishInterceptor<>).MakeGenericType(messageType);
        var abstractInterceptor = provider.Create(abstractInterceptorType, interceptorInstance);
        return (IAbstractPublishInterceptor)abstractInterceptor;
    }

    private static void ReplaceDefinition<T>(this IList<InterceptorDefinition<T>> list, InterceptorDefinition<T> definition, Type interceptorType)
        where T : class
    {
        var index = list.IndexOf(definition);
        list.RemoveAt(index);

        var factory = new InstanceCachingFactory<T>(p => (T)p.Create(interceptorType));
        list.Add(new InterceptorDefinition<T>(definition.MessageType, interceptorType, factory));
    }

    private static void ReplaceDefinition<T>(this IList<InterceptorDefinition<T>> list, InterceptorDefinition<T> definition, object replacementInstance)
        where T : class
    {
        var interceptorType = replacementInstance.GetType();
        var index = list.IndexOf(definition);
        list.RemoveAt(index);

        var factory = new InstanceCachingFactory<T>(_ => (T)replacementInstance);
        list.Add(new InterceptorDefinition<T>(definition.MessageType, interceptorType, factory));
    }
}
