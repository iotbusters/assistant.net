using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Messaging client configurations used during message handling.
    /// </summary>
    public sealed class MessagingClientOptions
    {
        /// <summary>
        ///     List of registered interceptors.
        /// </summary>
        public IList<Type> Interceptors { get; } = new List<Type>();
    }
}