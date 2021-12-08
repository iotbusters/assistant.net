using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Unions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    internal class MongoMessageHandlerProxy<TMessage, TResponse> : IAbstractHandler
        where TMessage : IMessage<TResponse>
    {
        private readonly ILogger logger;
        private readonly IDiagnosticContext context;
        private readonly ITypeEncoder typeEncoder;
        private readonly MongoHandlingClientOptions options;
        private readonly IMongoCollection<MongoRecord> collection;
        private readonly ISystemClock clock;
        private readonly ExceptionModelConverter converter;

        public MongoMessageHandlerProxy(
            ILogger<MongoMessageHandlerProxy<TMessage, TResponse>> logger,
            IOptions<MongoHandlingClientOptions> options,
            IDiagnosticContext context,
            ITypeEncoder typeEncoder,
            IMongoClientFactory clientFactory,
            ISystemClock clock,
            ExceptionModelConverter converter)
        {
            this.logger = logger;
            this.context = context;
            this.typeEncoder = typeEncoder;
            this.clock = clock;
            this.options = options.Value;
            this.collection = clientFactory
                .GetDatabase()
                .GetCollection<MongoRecord>(MongoNames.MessageCollectionName);
            this.converter = converter;
        }

        public async Task<object> Request(object message, CancellationToken token)
        {
            var attempt = 1;
            var strategy = options.ResponsePoll;

            var messageName = typeEncoder.Encode(message.GetType())
                              ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
            var messageId = message.GetSha1();
            await Publish((TMessage)message, token);
            await Task.Delay(strategy.DelayTime(attempt), token);

            while (true)
            {
                logger.LogDebug("Message({MessageName}/{MessageId}) polling: {Attempt} begins.", messageName, messageId, attempt);

                if (await FindOneResponded(messageId, token) is Some<MongoRecord>(var responded))
                {
                    logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} ends with {status}.", messageName, messageId, attempt, responded.Status);

                    if (responded.Status == HandlingStatus.Succeeded)
                        return (TResponse)responded.Response!;

                    if (responded.Status == HandlingStatus.Failed)
                        converter.ConvertFrom((ExceptionModel)responded.Response!)!.Throw();

                    throw new MessageContractException($"Not expected status {responded.Status}.");
                }

                logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} ends without response.", messageName, messageId, attempt);

                attempt++;
                if (!strategy.CanRetry(attempt))
                {
                    logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} won't proceed.", messageName, messageId, attempt);
                    break;
                }

                await Task.Delay(strategy.DelayTime(attempt), token);
            }

            throw new MessageDeferredException("No response from server in defined amount of time.");
        }

        public async Task Publish(object message, CancellationToken token)
        {
            var messageName = typeEncoder.Encode(message.GetType())
                              ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
            var messageId = message.GetSha1();
            var audit = new Audit { CorrelationId = context.CorrelationId, Requested = clock.UtcNow, User = context.User };

            var requested = new MongoRecord(messageId, messageName, message, audit);
            await InsertOne(requested, token);
        }

        private async Task InsertOne(MongoRecord record, CancellationToken token)
        {
            try
            {
                await collection.InsertOneAsync(
                    record,
                    new InsertOneOptions(),
                    token);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                logger.LogDebug("Message({MessageName}/{MessageId}) polling: already requested.", record.MessageName, record.Id);
            }
        }

        private async Task<Option<MongoRecord>> FindOneResponded(string id, CancellationToken token)
        {
            var existed = await collection
                .Find(filter: x => x.Id == id && x.Status != HandlingStatus.Requested, new FindOptions())
                .SingleOrDefaultAsync(token);
            return existed.AsOption();
        }
    }
}
