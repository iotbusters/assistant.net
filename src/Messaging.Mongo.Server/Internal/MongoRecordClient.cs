using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     MongoDB internal client implementation for read/write operations.
    /// </summary>
    internal class MongoRecordClient : IMongoRecordReader, IMongoRecordWriter
    {
        private readonly ILogger logger;
        private readonly IOptions<MongoHandlingServerOptions> options;
        private readonly ITypeEncoder typeEncoder;
        private readonly IMongoCollection<MongoRecord> collection;

        /// <summary/>
        public MongoRecordClient(
            ILogger<MongoRecordClient> logger,
            IOptions<MongoHandlingServerOptions> options,
            ITypeEncoder typeEncoder,
            IMongoClientFactory clientFactory)
        {
            this.logger = logger;
            this.options = options;
            this.typeEncoder = typeEncoder;
            this.collection = clientFactory.GetDatabase().GetCollection<MongoRecord>(MongoNames.MessageCollectionName);
        }

        /// <inheritdoc/>
        public async Task<Option<MongoRecord>> NextRequested(CancellationToken token)
        {
            logger.LogDebug("Lookup requested messages.");

            var messageNames = options.Value.MessageTypes.Select(typeEncoder.Encode).ToArray();

            try
            {
                var found = await collection
                    .Find(filter: x => messageNames.Contains(x.MessageName) && x.Status == HandlingStatus.Requested, new FindOptions())
                    .Limit(1)
                    .FirstOrDefaultAsync(token);

                if (found == null)
                {
                    logger.LogDebug("Found no message.");
                    return Option.None;
                }

                logger.LogDebug("Found a message.");
                return Option.Some(found);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lookup failed.");
                return Option.None;
            }
        }

        /// <inheritdoc/>
        public async Task Update(MongoRecord record, CancellationToken token)
        {
            try
            {
                var result = await collection.ReplaceOneAsync(
                    filter: x => x.Id == record.Id && x.Status == HandlingStatus.Requested,
                    record,
                    new ReplaceOptions(),
                    token);

                if (result.MatchedCount == 0)
                    logger.LogWarning("Message({MessageType}/{MessageId}) handling: already responded concurrently.", record.MessageName, record.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message({MessageType}/{MessageId}) handling: write error.", record.MessageName, record.Id);
            }
        }
    }
}
