using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
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
        private readonly MessagingClientOptions options;
        private readonly IMessagingClient client;
        private readonly ISystemClock clock;
        private readonly ExceptionModelConverter converter;

        /// <summary/>
        public MongoRecordProcessor(
            ILogger<MongoRecordProcessor> logger,
            IOptions<MessagingClientOptions> options,
            ExceptionModelConverter converter,
            IMessagingClient client,
            ISystemClock clock)
        {
            this.logger = logger;
            this.options = options.Value;
            this.converter = converter;
            this.client = client;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Option<MongoRecord>> Process(MongoRecord record, CancellationToken token)
        {
            logger.LogInformation("Message({MessageName}/{MessageId}) handling: started.", record.MessageName, record.Id);
            try
            {
                var response = await client.SendObject(record.Message, token);
                return record.Succeed(response, clock.UtcNow).AsOption();
            }
            catch (Exception ex)
            {
                if (ex is MessageDeferredException or TimeoutException or OperationCanceledException
                    || options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
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
