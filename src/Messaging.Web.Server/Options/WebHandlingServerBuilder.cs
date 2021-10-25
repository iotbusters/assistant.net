using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Type marker designed to be hooked by associated messaging handling extensions methods.
    /// </summary>
    public class WebHandlingServerBuilder
    {
        /// <summary/>
        public WebHandlingServerBuilder(IServiceCollection services) => Services = services;

        /// <summary/>
        public IServiceCollection Services { get; }
    }
}
