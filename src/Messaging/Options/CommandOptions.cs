using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Collects all commnad handler configurations which are required 
    ///     for resolving handlers by command type.
    /// </summary>
    public sealed class CommandOptions
    {
        [MinLength(1)]
        public ISet<HandlerDefinition> Handlers { get; } = new HashSet<HandlerDefinition>();
        public IList<InterceptorDefinition> Interceptors { get; } = new List<InterceptorDefinition>();
    }
}