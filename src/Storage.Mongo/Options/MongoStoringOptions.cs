using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Storage.Options
{
    /// <summary>
    ///     MongoDB based storage configuration.
    /// </summary>
    public class MongoStoringOptions
    {
        /// <summary>
        ///     Collection name for <see cref="IStorage{TKey,TValue}"/>.
        /// </summary>
        [Required]
        public string SingleCollectionName { get; set; } = MongoNames.StorageCollectionName;

        /// <summary>
        ///     Keys collection name for <see cref="IHistoricalStorage{TKey,TValue}"/> and <see cref="IPartitionedStorage{TKey,TValue}"/>.
        /// </summary>
        [Required]
        public string KeyCollectionName { get; set; } = MongoNames.HistoricalStorageKeyCollectionName;

        /// <summary>
        ///     Key-Values reference collection name for <see cref="IHistoricalStorage{TKey,TValue}"/> and <see cref="IPartitionedStorage{TKey,TValue}"/>.
        /// </summary>
        [Required]
        public string KeyValueCollectionName { get; set; } = MongoNames.HistoricalStorageKeyValueCollectionName;

        /// <summary>
        ///     Values collection name for <see cref="IHistoricalStorage{TKey,TValue}"/> and <see cref="IPartitionedStorage{TKey,TValue}"/>.
        /// </summary>
        [Required]
        public string ValueCollectionName { get; set; } = MongoNames.HistoricalStorageValueCollectionName;

        /// <summary>
        ///     Optimistic concurrent insert operation retrying strategy.
        /// </summary>
        [Required]
        public IRetryStrategy InsertRetry { get; set; } = new ConstantBackoff {Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 2};

        /// <summary>
        ///     Optimistic concurrent insert/replace operation retrying strategy.
        /// </summary>
        [Required]
        public IRetryStrategy UpsertRetry { get; set; } = new ConstantBackoff {Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 5};

        /// <summary>
        ///     Optimistic concurrent delete operation retrying strategy.
        /// </summary>
        public IRetryStrategy DeleteRetry { get; set; } = new ConstantBackoff { Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 5 };
    }
}
