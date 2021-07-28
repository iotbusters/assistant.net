using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Analyzers.Options
{
    /// <summary>
    ///     Required proxy configuration.
    /// </summary>
    public class ProxyFactoryOptions
    {
        private readonly HashSet<Type> proxyTypes = new();

        /// <summary>
        ///     Registers the <typeparamref name="T"/> proxy type required to proxy.
        /// </summary>
        public ProxyFactoryOptions Add<T>() => Add(typeof(T));

        /// <summary>
        ///     Registers the <paramref name="proxyType"/> required to proxy.
        /// </summary>
        public ProxyFactoryOptions Add(Type proxyType)
        {
            if (!proxyType.IsInterface)
                throw new ArgumentException($"Expected an interface type but provided `{proxyType.Name}` instead.", nameof(proxyType));

            proxyTypes.Add(proxyType);
            return this;
        }

        /// <summary>
        ///     Registered proxy types.
        /// </summary>
        internal Type[] ProxyTypes => proxyTypes.ToArray();
    }
}