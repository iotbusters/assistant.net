using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
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
        public async IAsyncEnumerable<MongoRecord> FindRequested([EnumeratorCancellation] CancellationToken token)
        {
            logger.LogDebug("Lookup requested messages.");

            var count = 0;
            while (!token.IsCancellationRequested)
            {
                MongoRecord? found;
                try
                {
                    found = await collection
                        .Find(filter: x => x.Status == HandlingStatus.Requested, new FindOptions())
                        .Limit(1)
                        .FirstOrDefaultAsync(token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Lookup failed.");
                    yield break;
                }

                if (found == null)
                    yield break;

                yield return found;
                count++;
            }

            logger.LogDebug("Found {Count} messages in total.", count);
        }

        /// <inheritdoc/>
        public async Task Update(MongoRecord record, CancellationToken token)
        {
            //var responseType = record.Response!.GetType();
            //if (!BsonClassMap.IsClassMapRegistered(responseType))
            //    BsonClassMap.LookupClassMap(responseType);

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
