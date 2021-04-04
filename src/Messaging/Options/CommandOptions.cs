using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Options
{
    public sealed class CommandOptions
    {
        [MinLength(1)]
        public ISet<HandlerDefinition> Handlers { get; } = new HashSet<HandlerDefinition>();
        public IList<InterceptorDefinition> Interceptors { get; } = new List<InterceptorDefinition>();
    }
}