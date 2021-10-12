using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
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
        private readonly IMongoCollection<MongoRecord> collection;

        /// <summary/>
        public MongoRecordClient(
            ILogger<MongoRecordClient> logger,
            IOptions<MongoHandlingServerOptions> options,
            IMongoClient client)
        {
            this.logger = logger;
            this.collection = client.GetDatabase(options.Value.DatabaseName).GetCollection<MongoRecord>(MongoNames.MessageCollectionName);
        }

        /// <inheritdoc/>
        public async Task<Option<MongoRecord>> NextRequested(CancellationToken token)
        {
            logger.LogDebug("Lookup requested messages.");

            try
            {
                var found = await collection
                    .Find(filter: x => x.Status == HandlingStatus.Requested, new FindOptions())
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
                    logger.LogWarning("Message({MessageType}/{MessageId}) handling: already responded concurrently.", record.Name, record.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message({MessageType}/{MessageId}) handling: write error.", record.Name, record.Id);
            }
        }
    }
}
