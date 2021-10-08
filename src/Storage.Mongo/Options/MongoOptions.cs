using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Storage.Options
{
    /// <summary>
    ///     MongoDB client configurations used for specific storage usage.
    /// </summary>
    public class MongoOptions
    {
        /// <summary>
        ///     MongoDB server connection string.
        /// </summary>
        [Required, MinLength(10)]// mongodb://
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        ///     MongoDB database name.
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = "Storage";

        /// <summary>
        ///     Max attempt number for optimistic concurrent insert operation.
        /// </summary>
        [Range(minimum: 1, maximum: int.MaxValue)]
        public int MaxInsertAttemptNumber { get; set; } = 2;

        /// <summary>
        ///     Max attempt number for optimistic concurrent replace/insert operation.
        /// </summary>
        [Range(minimum: 1, maximum: int.MaxValue)]
        public int MaxUpsertAttemptNumber { get; set; } = 5;

        /// <summary>
        ///     Time to delay before retry optimistic concurrent operation.
        /// </summary>
        public TimeSpan OptimisticConcurrencyDelayTime { get; set; } = TimeSpan.FromSeconds(0.01);
    }
}
