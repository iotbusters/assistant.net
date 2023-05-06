using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures a single message <paramref name="handlerFactory"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the message handler will be used only for message types registered by calling one of <see cref="AddSingle"/> methods.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="handlerFactory">A single message handler instance factory.</param>
    public static MessagingClientOptions UseSingleHandler(this MessagingClientOptions options, Func<IServiceProvider, Type, IAbstractHandler> handlerFactory)
    {
        options.SingleHandlerFactory = new InstanceCachingFactory<IAbstractHandler, Type>(handlerFactory);
        return options;
    }

    /// <summary>
    ///     Registers single handler based handler of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of <see cref="UseSingleHandler"/> like methods.
    /// </remarks>
    /// <typeparam name="TMessage">A specific message type to be handled by a single handler.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddSingle<TMessage>(this MessagingClientOptions options)
        where TMessage : IAbstractMessage => options
        .AddSingle(typeof(TMessage));

    /// <summary>
    ///     Registers the single handler for <paramref name="messageType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of <see cref="UseSingleHandler"/> like methods.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="messageType">A specific message type to be handled by a single handler.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddSingle(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.HandlerFactories[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
        {
            var definition = options.SingleHandlerFactory ?? throw new HandlerNotRegisteredException();
            return definition.Create(p, messageType);
        });

        return options;
    }

    /// <summary>
    ///     Configures <paramref name="backoffHandler"/> instance.
    /// </summary>
    /// <remarks>
    ///     The message handler is used if no other handlers were configured for a message type.
    ///     Pay attention, the method overrides already registered backoff handler.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="backoffHandler">A backoff message handler instance.</param>
    public static MessagingClientOptions UseBackoffHandler(this MessagingClientOptions options, IAbstractHandler backoffHandler) => options
        .UseBackoffHandler((_, _) => backoffHandler);

    /// <summary>
    ///     Configures <paramref name="handlerFactory"/>.
    /// </summary>
    /// <remarks>
    ///     The message handler is used if no other handlers were configured for a message type.
    ///     Pay attention, the method overrides already registered backoff handler.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="handlerFactory">A backoff message handler instance factory.</param>
    public static MessagingClientOptions UseBackoffHandler(this MessagingClientOptions options, Func<IServiceProvider, Type, IAbstractHandler> handlerFactory)
    {
        options.BackoffHandlerFactory = new InstanceCachingFactory<IAbstractHandler, Type>(handlerFactory);
        return options;
    }

    /// <summary>
    ///     Registers a named message handler type and allows messages it can handle.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="handlerType">A message handler type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddHandler(this MessagingClientOptions options, Type handlerType)
    {
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        foreach (var messageType in messageTypes)
            options.HandlerFactories[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            {
                var handlerInstance = p.Create(handlerType);
                var providerType = typeof(LocalMessageHandlingProxy<,>).MakeGenericTypeBoundToMessage(messageType);
                var provider = p.Create(providerType, handlerInstance);
                return (IAbstractHandler)provider;
            });

        return options;
    }

    /// <summary>
    ///     Registers a named message handler instance and allows messages it can handle.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers.
    /// </remarks>
    /// <param name="options"/>
    /// <param name="handlerInstance">A message handler instance.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddHandler(this MessagingClientOptions options, object handlerInstance)
    {
        var handlerType = handlerInstance.GetType();
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

        foreach (var messageType in messageTypes)
            options.HandlerFactories[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            {
                var providerType = typeof(LocalMessageHandlingProxy<,>).MakeGenericTypeBoundToMessage(messageType);
                var handler = p.Create(providerType, handlerInstance);
                return (IAbstractHandler)handler;
            });

        return options;
    }

    /// <summary>
    ///     Removes the <paramref name="handlerType"/> and disallows messages it can handle.
    /// </summary>
    /// <param name="options"/>
    /// <param name="handlerType">A message handler type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions RemoveHandler(this MessagingClientOptions options, Type handlerType)
    {
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        foreach (var messageType in messageTypes)
            options.HandlerFactories.Remove(messageType);

        return options;
    }

    /// <summary>
    ///     Allows exposing an external <typeparamref name="TException"/> type.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions ExposeException<TException>(this MessagingClientOptions options)
        where TException : Exception => options.ExposeException(typeof(TException));

    /// <summary>
    ///     Allows exposing an external <paramref name="exceptionType"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions ExposeException(this MessagingClientOptions options, Type exceptionType)
    {
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

        options.ExposedExceptions.Add(exceptionType);
        return options;
    }

    /// <summary>
    ///     Removes an exposed <typeparamref name="TException"/> type.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions RemoveExposedException<TException>(this MessagingClientOptions options)
        where TException : Exception => options.RemoveExposedException(typeof(TException));

    /// <summary>
    ///     Removes an exposed <paramref name="exceptionType"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions RemoveExposedException(this MessagingClientOptions options, Type exceptionType)
    {
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

        options.ExposedExceptions.Remove(exceptionType);
        return options;
    }

    /// <summary>
    ///     Removes all exposed external exception types.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions ClearExposedExceptions(this MessagingClientOptions options)
    {
        options.ExposedExceptions.Clear();
        return options;
    }

    /// <summary>
    ///     Allows retrying a transient <typeparamref name="TException"/> type.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions AddTransientException<TException>(this MessagingClientOptions options)
        where TException : Exception => options.AddTransientException(typeof(TException));

    /// <summary>
    ///     Allows retrying a transient <paramref name="exceptionType"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddTransientException(this MessagingClientOptions options, Type exceptionType)
    {
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

        options.TransientExceptions.Add(exceptionType);
        return options;
    }

    /// <summary>
    ///     Removes a transient <typeparamref name="TException"/> type.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions RemoveTransientException<TException>(this MessagingClientOptions options)
        where TException : Exception => options.RemoveTransientException(typeof(TException));

    /// <summary>
    ///     Removes a transient <paramref name="exceptionType"/>.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions RemoveTransientException(this MessagingClientOptions options, Type exceptionType)
    {
        if (!exceptionType.IsAssignableTo(typeof(Exception)))
            throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

        options.TransientExceptions.Remove(exceptionType);
        return options;
    }

    /// <summary>
    ///     Removes all transient exception types.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions ClearTransientExceptions(this MessagingClientOptions options)
    {
        options.TransientExceptions.Clear();
        return options;
    }

    /// <summary>
    ///     Appends the <paramref name="interceptorType"/> to the list.
    /// </summary>
    /// <param name="options"/>
    /// <param name="interceptorType">A message interceptor type.</param>
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
    ///     Overrides the retrying strategy.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions Retry(this MessagingClientOptions options, IRetryStrategy strategy)
    {
        options.Retry = strategy;
        return options;
    }

    /// <summary>
    ///     Overrides the retrying strategy.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="RetryingInterceptor"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions Retry(this MessagingClientOptions options, IConfigurationSection configuration) => options
        .Retry(IRetryStrategy.ReadStrategy(configuration));

    /// <summary>
    ///     Overrides the message handling timeout.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="TimeoutInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions TimeoutIn(this MessagingClientOptions options, TimeSpan timeout)
    {
        options.Timeout = timeout;
        return options;
    }

    /// <summary>
    ///     Disables the message handling timeout.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="TimeoutInterceptor"/>.
    /// </remarks>
    public static MessagingClientOptions NoTimeout(this MessagingClientOptions options) => options
        .TimeoutIn(Timeout.InfiniteTimeSpan);

    /// <summary>
    ///     Appends the <paramref name="interceptorInstance"/> to the list.
    /// </summary>
    /// <param name="options"/>
    /// <param name="interceptorInstance">A message interceptor instance.</param>
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
    /// <param name="targetType">A message interceptor type to be replaced.</param>
    /// <param name="replacementType">A message interceptor type to be used instead.</param>
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
    /// <param name="targetType">A message interceptor type to be replaced.</param>
    /// <param name="replacementInstance">A message interceptor instance to be used instead.</param>
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
    /// <param name="interceptorType">A message interceptor type.</param>
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
            list.Add(new(messageType, interceptorType, factory));
        }
    }

    private static void AddAbstractInterceptor(this IList<InterceptorDefinition<IAbstractPublishInterceptor>> list, Type interceptorType)
    {
        var newMessageTypes = list.GetUndefinedMessageTypes(interceptorType);
        foreach (var messageType in newMessageTypes)
        {
            var factory = new InstanceCachingFactory<IAbstractPublishInterceptor>(p =>
                CreateAbstractPublishInterceptor(p, messageType, p.Create(interceptorType)));
            list.Add(new(messageType, interceptorType, factory));
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
            list.Add(new(messageType, interceptorType, factory));
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
            list.Add(new(messageType, interceptorType, factory));
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
        list.Add(new(definition.MessageType, interceptorType, factory));
    }

    private static void ReplaceDefinition<T>(this IList<InterceptorDefinition<T>> list, InterceptorDefinition<T> definition, object replacementInstance)
        where T : class
    {
        var interceptorType = replacementInstance.GetType();
        var index = list.IndexOf(definition);
        list.RemoveAt(index);

        var factory = new InstanceCachingFactory<T>(_ => (T)replacementInstance);
        list.Add(new(definition.MessageType, interceptorType, factory));
    }
}
