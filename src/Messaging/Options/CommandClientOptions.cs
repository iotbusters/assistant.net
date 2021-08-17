using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Collects all command client configurations which are required during command handling.
    /// </summary>
    public sealed class CommandClientOptions
    {
        /// <summary>
        ///     List of registered interceptors.
        /// </summary>
        public IList<Type> Interceptors { get; } = new List<Type>();
    }
}