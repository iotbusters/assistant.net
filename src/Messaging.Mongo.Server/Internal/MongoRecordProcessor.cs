using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Message processing implementation.
    /// </summary>
    internal class MongoRecordProcessor : IMongoRecordProcessor
    {
        private readonly ILogger logger;
        private readonly IOptionsMonitor<MessagingClientOptions> options;
        private readonly IMessagingClientFactory clientFactory;
        private readonly ISystemClock clock;
        private readonly ExceptionModelConverter converter;

        /// <summary/>
        public MongoRecordProcessor(
            ILogger<MongoRecordProcessor> logger,
            IOptionsMonitor<MessagingClientOptions> options,
            ExceptionModelConverter converter,
            IMessagingClientFactory clientFactory,
            ISystemClock clock)
        {
            this.logger = logger;
            this.options = options;
            this.converter = converter;
            this.clientFactory = clientFactory;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Option<MongoRecord>> Process(MongoRecord record, CancellationToken token)
        {
            logger.LogInformation("Message({MessageName}/{MessageId}) handling: started.", record.MessageName, record.Id);
            try
            {
                var client = clientFactory.Create(MongoOptionsNames.DefaultName);
                var response = await client.RequestObject(record.Message, token);
                return record.Succeed(response, clock.UtcNow).AsOption();
            }
            catch (Exception ex)
            {
                if (ex is MessageDeferredException or TimeoutException or OperationCanceledException
                    || options.CurrentValue.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                {
                    logger.LogInformation(ex, "Message({MessageType}/{MessageId}) handling: deferred or transient error.", record.MessageName, record.Id);
                    return Option.None;
                }

                logger.LogError(ex, "Message({MessageType}/{MessageId}) handling: permanent error.", record.MessageName, record.Id);
                return record.Fail(converter.ConvertTo(ex)!, clock.UtcNow).AsOption();
            }
        }
    }
}
