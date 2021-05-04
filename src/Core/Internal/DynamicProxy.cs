using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Assistant.Net.Internal
{
    /// <summary>
    ///     Dynamic proxy implementation.
    /// </summary>
    /// <typeparam name="T">The interface which is going to be decorated.</typeparam>
    /// <see cref="https://devblogs.microsoft.com/dotnet/migrating-realproxy-usage-to-dispatchproxy/" />
    /// <see cref="https://github.com/dotnet/corefx/blob/master/src/System.Reflection.DispatchProxy/src/System/Reflection/DispatchProxy.cs"/>
    internal class DynamicProxy<T> : DispatchProxy
    {
        private readonly List<Interceptor> interceptors = new List<Interceptor>(2);
        private T instance = default!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            // todo: consider reusing commands and their interceptors. Ensure performance (https://github.com/iotbusters/assistant.net/issues/2)
            interceptors
                .Select(intercept => (intercepted: intercept(targetMethod!, args!, out var result), result))
                .First(x => x.intercepted).result;

        public static DynamicProxy<T> Create() => (DynamicProxy<T>)(object)Create<T, DynamicProxy<T>>()!;

        public DynamicProxy<T> AddInterceptor(Interceptor interceptor)
        {
            interceptors.Add(interceptor);
            return this;
        }

        public DynamicProxy<T> AddDecorator(Decorator decorator)
        {
            interceptors.Add((MethodInfo method, object?[] args, out object? result) =>
            {
                result = decorator(method, args);
                return false;
            });
            return this;
        }

        public T Decorate(T @object)
        {
            instance = @object;
            interceptors.Reverse();
            interceptors.Add((MethodInfo method, object?[] args, out object? result) =>
            {
                result = method.Invoke(instance, args);
                return true;
            });
            return (T) (object) this;
        }
    }

    delegate bool Interceptor(MethodInfo targetMethod, object?[] args, out object? result);
    delegate object? Decorator(MethodInfo targetMethod, object?[] args);
}