using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Collects all commnad client configurations which are required during command handling.
    /// </summary>
    public sealed class CommandClientOptions
    {
        public IList<Type> Interceptors { get; } = new List<Type>();
    }
}