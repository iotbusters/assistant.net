using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     MongoDB based message handling configuration builder on a server.
    /// </summary>
    public class MongoHandlingBuilder : MessagingClientBuilder<MongoHandlingBuilder>
    {
        /// <summary/>
        public MongoHandlingBuilder(IServiceCollection services) : base(services, MongoOptionsNames.DefaultName) { }
    }
}
