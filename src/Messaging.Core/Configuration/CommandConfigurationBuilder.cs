using System.Collections.Generic;

namespace Assistant.Net.Messaging.Configuration
{
    public sealed class CommandConfigurationBuilder
    {
        public List<InterceptorDefinition> Interceptors { get; } = new();
        public List<HandlerDefinition> Handlers { get; } = new();
    }
}