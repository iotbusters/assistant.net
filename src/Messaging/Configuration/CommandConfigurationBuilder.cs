using System;
using System.Collections.Generic;
using System.Linq;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Configuration
{
    public sealed class CommandConfigurationBuilder
    {
        public List<InterceptorDefinition> Interceptors { get; } = new();
        public List<HandlerDefinition> Handlers { get; } = new();
    }
}