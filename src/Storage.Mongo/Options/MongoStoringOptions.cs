﻿using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
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
        ///     MongoDB database name.
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = MongoNames.DatabaseName;

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
    }
}
