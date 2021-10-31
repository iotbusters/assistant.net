using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Messaging client configuration extensions.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Registers a local in-memory handler type <typeparamref name="THandler" />.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddLocalHandler<THandler>(this MessagingClientBuilder builder)
            where THandler : class => builder.AddLocalHandler(typeof(THandler));

        /// <summary>
        ///     Registers a local in-memory <paramref name="handlerType" />.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddLocalHandler(this MessagingClientBuilder builder, Type handlerType)
        {
            var handlerInterfaceTypes = handlerType.GetMessageHandlerInterfaceTypes();
            if (!handlerInterfaceTypes.Any())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));
            
            foreach (var handlerInterfaceType in handlerInterfaceTypes)
                builder.Services.ReplaceTransient(handlerInterfaceType, handlerType);
            return builder;
        }

        /// <summary>
        ///     Registers a local in-memory <paramref name="handlerInstance" />.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddLocalHandler(this MessagingClientBuilder builder, object handlerInstance)
        {
            var handlerType = handlerInstance.GetType();
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

            var handlerInterfaceTypes = handlerType.GetMessageHandlerInterfaceTypes();
            foreach (var handlerInterfaceType in handlerInterfaceTypes)
                builder.Services.Replace(ServiceDescriptor.Singleton(handlerInterfaceType, _ => handlerInstance));
            return builder;
        }

        /// <summary>
        ///     Apply a configuration type <typeparamref name="TConfiguration" />.
        /// </summary>
        public static MessagingClientBuilder AddConfiguration<TConfiguration>(this MessagingClientBuilder builder)
            where TConfiguration : IMessageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

        /// <summary>
        ///     Apply a list of configuration instances <paramref name="messageConfigurations" />.
        /// </summary>
        public static MessagingClientBuilder AddConfiguration(this MessagingClientBuilder builder, params IMessageConfiguration[] messageConfigurations)
        {
            foreach (var config in messageConfigurations)
                config.Configure(builder);
            return builder;
        }

        /// <summary>
        ///     Adds an interceptor type <typeparamref name="TInterceptor" /> to the end of the list.
        /// </summary>
        public static MessagingClientBuilder AddInterceptor<TInterceptor>(this MessagingClientBuilder builder)
            where TInterceptor : class => builder
            .AddInterceptor(typeof(TInterceptor));

        /// <summary>
        ///     Adds the <paramref name="interceptorType" /> to the end of the list.
        /// </summary>
        public static MessagingClientBuilder AddInterceptor(this MessagingClientBuilder builder, Type interceptorType)
        {
            if (!interceptorType.IsMessageInterceptor())
                throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

            builder.Services
                .ConfigureMessagingClientOptions(o => o.Interceptors.Add(interceptorType))
                .TryAddTransient(interceptorType, interceptorType);
            return builder;
        }

        /// <summary>
        ///     Adds the <paramref name="interceptorInstance" /> to the end of the list.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddInterceptor(this MessagingClientBuilder builder, object interceptorInstance)
        {
            var interceptorType = interceptorInstance.GetType();
            if (!interceptorType.IsMessageHandler())
                throw new ArgumentException($"Expected message interceptor but provided {interceptorType}.", nameof(interceptorInstance));

            builder.Services
                .ConfigureMessagingClientOptions(o => o.Interceptors.Add(interceptorType))
                .Replace(ServiceDescriptor.Singleton(interceptorType, _ => interceptorInstance));
            return builder;
        }

        /// <summary>
        ///     Adds an interceptor type <typeparamref name="TInterceptor" /> at the beginning of the list.
        /// </summary>
        public static MessagingClientBuilder AddInterceptorOnTop<TInterceptor>(this MessagingClientBuilder builder)
            where TInterceptor : class
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Interceptors.Insert(0, typeof(TInterceptor)));
            return builder;
        }

        /// <summary>
        ///     Removes all interceptors from the list.
        /// </summary>
        public static MessagingClientBuilder ClearInterceptors(this MessagingClientBuilder builder)
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Interceptors.Clear());
            return builder;
        }

        /// <summary>
        ///     Removes an interceptor type <typeparamref name="TInterceptor" /> from the list.
        /// </summary>
        public static MessagingClientBuilder RemoveInterceptor<TInterceptor>(this MessagingClientBuilder builder)
            where TInterceptor : class => builder
            .RemoveInterceptor(typeof(TInterceptor));

        /// <summary>
        ///     Removes an interceptor type <paramref name="interceptorType" /> from the list.
        /// </summary>
        public static MessagingClientBuilder RemoveInterceptor(this MessagingClientBuilder builder, Type interceptorType)
        {
            if (!interceptorType.IsMessageInterceptor())
                throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

            builder.Services.ConfigureMessagingClientOptions(o => o.Interceptors.Remove(interceptorType));
            return builder;
        }

        /// <summary>
        ///     Allows exposing the external exception type <typeparamref name="TException" />.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="ErrorHandlingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder ExposeException<TException>(this MessagingClientBuilder builder)
            where TException : Exception => builder
            .ExposeException(typeof(TException));

        /// <summary>
        ///     Allows exposing the external <paramref name="exceptionType"/>.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="ErrorHandlingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder ExposeException(this MessagingClientBuilder builder, Type exceptionType)
        {
            if (!exceptionType.IsAssignableTo(typeof(Exception)))
                throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

            builder.Services.ConfigureMessagingClientOptions(o => o.ExposedExceptions.Add(exceptionType));
            return builder;
        }

        /// <summary>
        ///     Removes the exposed external exception type <typeparamref name="TException" />.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="ErrorHandlingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder RemoveExposedException<TException>(this MessagingClientBuilder builder)
            where TException : Exception
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.ExposedExceptions.Remove(typeof(TException)));
            return builder;
        }

        /// <summary>
        ///     Removes all exposed external exception types.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="ErrorHandlingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder ClearExposedExceptions(this MessagingClientBuilder builder)
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.ExposedExceptions.Clear());
            return builder;
        }

        /// <summary>
        ///     Allows retrying the transient exception type <typeparamref name="TException" />.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder AddTransientException<TException>(this MessagingClientBuilder builder)
            where TException : Exception => builder
            .AddTransientException(typeof(TException));

        /// <summary>
        ///     Allows retrying the transient <paramref name="exceptionType" />.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder AddTransientException(this MessagingClientBuilder builder, Type exceptionType)
        {
            if (!exceptionType.IsAssignableTo(typeof(Exception)))
                throw new ArgumentException($"Expected exception but provided {exceptionType}.", nameof(exceptionType));

            builder.Services.ConfigureMessagingClientOptions(o => o.TransientExceptions.Add(exceptionType));
            return builder;
        }

        /// <summary>
        ///     Removes the transient exception type <typeparamref name="TException" />.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder RemoveTransientException<TException>(this MessagingClientBuilder builder)
            where TException : Exception
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.TransientExceptions.Remove(typeof(TException)));
            return builder;
        }

        /// <summary>
        ///     Removes all transient exception types.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder ClearTransientExceptions(this MessagingClientBuilder builder)
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.TransientExceptions.Clear());
            return builder;
        }

        /// <summary>
        ///     Overrides retrying strategy.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="RetryingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder Retry(this MessagingClientBuilder builder, IRetryStrategy strategy)
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Retry = strategy);
            return builder;
        }

        /// <summary>
        ///     Overrides retrying strategy.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="RetryingInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder Retry(this MessagingClientBuilder builder, IConfigurationSection configuration)
        {
            IRetryStrategy strategy = configuration["type"] switch
            {
                "Exponential"   => configuration.Get<ExponentialBackoff>(),
                "Linear"        => configuration.Get<LinearBackoff>(),
                "Constant"      => configuration.Get<ConstantBackoff>(),
                _               => throw new ArgumentException($"Key 'type' at {configuration.Path} is expected to be: "
                                                               + $"'Exponential', 'Linear' or 'Constant' but was '{configuration["type"]}'.")
            };
            builder.Services.ConfigureMessagingClientOptions(o => o.Retry = strategy);
            return builder;
        }

        /// <summary>
        ///     Overrides message handling timeout.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="TimeoutInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder TimeoutIn(this MessagingClientBuilder builder, TimeSpan timeout)
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Timeout = timeout);
            return builder;
        }
    }
}
