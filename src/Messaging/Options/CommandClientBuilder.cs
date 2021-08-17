using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Command client building container for <see cref="CommandClientExtensions"/>.
    /// </summary>
    public class CommandClientBuilder
    {
        /// <summary/>
        public CommandClientBuilder(IServiceCollection services) => Services = services;

        /// <summary/>
        public IServiceCollection Services { get; }
    }
}