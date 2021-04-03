using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Configuration
{
    public sealed class CommandOptions
    {
        [MinLength(1)]
        public ISet<HandlerDefinition> Handlers { get; } = new HashSet<HandlerDefinition>();
        public IList<InterceptorDefinition> Interceptors { get; } = new List<InterceptorDefinition>();
    }
}