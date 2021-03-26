using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Configuration
{
    public sealed class CommandOptions
    {
        public IEnumerable<KeyValuePair<Type, Type>> Interceptors { get; internal set; }
        public IEnumerable<KeyValuePair<Type, Type>> Handlers { get; internal set; }
    }
}