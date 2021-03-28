using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Configuration
{
    public sealed class CommandOptions
    {
        public List<Type> Handlers { get; } = new();
        public List<Type> Interceptors { get; } = new();
    }
}