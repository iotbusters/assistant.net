using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Type marker designed to be hooked by associated messaging extensions methods.
    /// </summary>
    public class MessagingClientBuilder
    {
        /// <summary/>
        public MessagingClientBuilder(IServiceCollection services) => Services = services;

        /// <summary/>
        public IServiceCollection Services { get; }
    }
}