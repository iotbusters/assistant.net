using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Type marker of abstract messaging client builder.
    /// </summary>
    public interface IMessagingClientBuilder
    {
        /// <summary>
        ///     The name of the <see cref="MessagingClientOptions"/> instance.
        /// </summary>
        public string Name { get; }

        /// <summary/>
        public IServiceCollection Services { get; }
    }
}
