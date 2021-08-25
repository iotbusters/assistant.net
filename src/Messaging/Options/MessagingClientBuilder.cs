using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Messaging client building container for <see cref="MessagingClientExtensions"/>.
    /// </summary>
    public class MessagingClientBuilder
    {
        /// <summary/>
        public MessagingClientBuilder(IServiceCollection services) => Services = services;

        /// <summary/>
        public IServiceCollection Services { get; }
    }
}