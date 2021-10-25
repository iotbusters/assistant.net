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
    internal class MongoMessageHandlerProxy<TMessage, TResponse> : IMessageHandler<TMessage, TResponse>
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
            this.collection = clientFactory.Create().GetDatabase(this.options.DatabaseName).GetCollection<MongoRecord>(MongoNames.MessageCollectionName);
            this.converter = converter;
        }

        public async Task<TResponse> Handle(TMessage message, CancellationToken token)
        {
            var strategy = options.ResponsePoll;

            var attempt = 1;

            var messageName = typeEncoder.Encode(message.GetType())
                              ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
            var audit = new Audit {CorrelationId = context.CorrelationId, Requested = clock.UtcNow, User = context.User};
            var requested = new MongoRecord(message.GetSha1(), messageName, message, audit);
            if (await InsertOne(requested, token))
                await Task.Delay(strategy.DelayTime(attempt), token);

            while (true)
            {
                logger.LogDebug("Message({MessageName}/{MessageId}) polling: {Attempt} begins.", requested.MessageName, requested.Id, attempt);

                if (await FindOneResponded(requested.Id, token) is Some<MongoRecord>(var responded))
                {
                    logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} ends with {status}.", requested.MessageName, requested.Id, attempt, responded.Status);

                    if (responded.Status == HandlingStatus.Succeeded)
                        return (TResponse)responded.Response!;

                    if (responded.Status == HandlingStatus.Failed)
                        converter.ConvertFrom((ExceptionModel)responded.Response!)!.Throw();

                    throw new MessageContractException($"Not expected status {responded.Status}.");
                }

                logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} ends without response.", requested.MessageName, requested.Id, attempt);

                attempt++;
                if (!strategy.CanRetry(attempt))
                {
                    logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} won't proceed.", requested.MessageName, requested.Id, attempt);
                    break;
                }

                await Task.Delay(strategy.DelayTime(attempt), token);
            }

            throw new MessageDeferredException();
        }

        private async Task<bool> InsertOne(MongoRecord record, CancellationToken token)
        {
            try
            {
                await collection.InsertOneAsync(
                    record,
                    new InsertOneOptions(),
                    token);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                logger.LogDebug("Message({MessageName}/{MessageId}) polling: already requested.", record.MessageName, record.Id);
                return false;
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
