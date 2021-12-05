using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.Configuration;
using System;

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
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddLocalHandler<THandler>(this MessagingClientBuilder builder)
            where THandler : class => builder.AddLocalHandler(typeof(THandler));

        /// <summary>
        ///     Registers a local in-memory <paramref name="handlerType" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
        /// <param name="handlerType">The message handler implementation type.</param>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddLocalHandler(this MessagingClientBuilder builder, Type handlerType)
        {
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddLocalHandler(handlerType));
            return builder;
        }

        /// <summary>
        ///     Registers a local in-memory <paramref name="handlerInstance" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
        /// <param name="handlerInstance">The message handler implementation instance.</param>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddLocalHandler(this MessagingClientBuilder builder, object handlerInstance)
        {
            var handlerType = handlerInstance.GetType();
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddLocalHandler(handlerInstance));
            return builder;
        }

        /// <summary>
        ///     Removes all handlers.
        /// </summary>
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
        public static MessagingClientBuilder ClearHandlers(this MessagingClientBuilder builder)
        {
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Handlers.Clear());
            return builder;
        }

        /// <summary>
        ///     Removes an handler of <typeparamref name="TMessage" />.
        /// </summary>
        /// <typeparam name="TMessage">The message type to find associated handler.</typeparam>
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
        public static MessagingClientBuilder Remove<TMessage>(this MessagingClientBuilder builder)
            where TMessage : class => builder
            .Remove(typeof(TMessage));

        /// <summary>
        ///     Removes an handler of <paramref name="messageType" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
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
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
        public static MessagingClientBuilder RemoveHandler<THandler>(this MessagingClientBuilder builder)
            where THandler : class => builder.RemoveHandler(typeof(THandler));

        /// <summary>
        ///     Removes the <paramref name="handlerType" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMessagingClient"/> builder.</param>
        /// <param name="handlerType">The message handler implementation type.</param>
        public static MessagingClientBuilder RemoveHandler(this MessagingClientBuilder builder, Type handlerType)
        {
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.RemoveHandler(handlerType));
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

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddInterceptor(interceptorType));
            return builder;
        }

        /// <summary>
        ///     Adds the <paramref name="interceptorInstance" /> to the end of the list.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddInterceptor(this MessagingClientBuilder builder, object interceptorInstance)
        {
            var interceptorType = interceptorInstance.GetType();
            if (!interceptorType.IsMessageInterceptor())
                throw new ArgumentException($"Expected message interceptor but provided {interceptorType}.", nameof(interceptorInstance));

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddInterceptor(interceptorInstance));
            return builder;
        }

        /// <summary>
        ///     Removes all interceptors.
        /// </summary>
        public static MessagingClientBuilder ClearInterceptors(this MessagingClientBuilder builder)
        {
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Interceptors.Clear());
            return builder;
        }

        /// <summary>
        ///     Removes an interceptor type <typeparamref name="TInterceptor" />.
        /// </summary>
        public static MessagingClientBuilder RemoveInterceptor<TInterceptor>(this MessagingClientBuilder builder)
            where TInterceptor : class => builder
            .RemoveInterceptor(typeof(TInterceptor));

        /// <summary>
        ///     Removes an interceptor type <paramref name="interceptorType" />.
        /// </summary>
        public static MessagingClientBuilder RemoveInterceptor(this MessagingClientBuilder builder, Type interceptorType)
        {
            if (!interceptorType.IsMessageInterceptor())
                throw new ArgumentException($"Expected interceptor but provided {interceptorType}.", nameof(interceptorType));

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.RemoveInterceptor(interceptorType));
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

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ExposedExceptions.Add(exceptionType));
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
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ExposedExceptions.Remove(typeof(TException)));
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
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.ExposedExceptions.Clear());
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

            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.TransientExceptions.Add(exceptionType));
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
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.TransientExceptions.Remove(typeof(TException)));
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
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.TransientExceptions.Clear());
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
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Retry = strategy);
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
            return builder.Retry(strategy);
        }

        /// <summary>
        ///     Overrides message handling timeout.
        /// </summary>
        /// <remarks>
        ///     Impacts <see cref="TimeoutInterceptor"/>.
        /// </remarks>
        public static MessagingClientBuilder TimeoutIn(this MessagingClientBuilder builder, TimeSpan timeout)
        {
            builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.Timeout = timeout);
            return builder;
        }
    }
}
