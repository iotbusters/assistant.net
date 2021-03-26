using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging
{
    public sealed class CommandClientInterceptableProxy : ICommandClient
    {
        private readonly ICommandClient client;
        private readonly ISystemLifetime lifetime;
        private readonly IServiceProvider provider;
        private readonly IEnumerable<KeyValuePair<Type, Type>> interceptorMap;

        public CommandClientInterceptableProxy(
            ICommandClient client,
            ISystemLifetime lifetime,
            IServiceProvider provider,
            IOptions<CommandOptions> options)
        {
            this.client = client;
            this.lifetime = lifetime;
            this.provider = provider;
            this.interceptorMap = options.Value.Interceptors;
        }

        public Task<TResponse> Send<TResponse>(ICommand<TResponse> command)
        {
            var commandType = command.GetType();
            var interceptors = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(commandType))
                .Select(x => provider.GetRequiredService(x.Value))
                .Reverse();
            // foreach (var item in interceptors)
            // {
            //     var method = item.GetType().GetMethod(nameof(ICommandInterceptor.Intercept));
            //     method.Invoke(item, new object[] { command });
            // }
            return interceptors
                .Aggregate(
                    new Func<ICommand<TResponse>, Task<TResponse>>(client.Send),
                    ChainInterceptors)
                .Invoke(command);
        }

        private Func<ICommand<TResponse>, Task<TResponse>> ChainInterceptors<TResponse>
            (Func<ICommand<TResponse>, Task<TResponse>> next, dynamic current) =>
                async x =>
                {
                    lifetime.Stopping.ThrowIfCancellationRequested();
                    MethodInfo method = current.GetType().GetMethod(nameof(ICommandInterceptor.Intercept));
                    var newNextType = method.GetParameters().Last().ParameterType;
                    var newCommandType = newNextType.GetGenericArguments().First();
                    var newResponseType = newNextType.GetGenericArguments().Last();
                    var b = newResponseType.GetGenericArguments().Single();
                    // Delegate.CreateDelegate(newNextType, )
                    // var task = (Task)method.Invoke(current, new object[] { x, next });
                    // await task;
                    // return (TResponse)task.GetType().GetProperty(nameof(Task<>.Result)).GetValue(task);
                    var mapType = GetType().GetMethod(nameof(Map)).MakeGenericMethod(typeof(TResponse), b);
                    return (TResponse)await current.Intercept(x, mapType.Invoke(null, new object[] { next }));
                };
        private static Func<ICommand<T2>, Task<T2>> Map<T1, T2>(Func<ICommand<T1>, Task<T1>> next) =>
            new(async x => (T2)(object)await next((ICommand<T1>)x));
    }
}