using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Type marker designed to be hooked by associated messaging handling extensions methods.
    /// </summary>
    public class MongoHandlingServerBuilder
    {
        /// <summary/>
        public MongoHandlingServerBuilder(IServiceCollection services) => Services = services;

        /// <summary/>
        public IServiceCollection Services { get; }
    }
}
