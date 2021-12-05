using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Type marker designed to be hooked by associated messaging extensions methods.
    /// </summary>
    public class MessagingClientBuilder
    {
        /// <summary/>
        public MessagingClientBuilder(string name, IServiceCollection services)
        {
            Name = name;
            Services = services;
        }

        /// <summary>
        ///     The name of the <see cref="MessagingClientOptions"/> instance.
        /// </summary>
        public string Name { get; }

        /// <summary/>
        public IServiceCollection Services { get; }
    }
}
