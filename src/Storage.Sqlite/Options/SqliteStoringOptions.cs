using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Storage.Options;

/// <summary>
///     SQLite client configuration for specific storage usage.
/// </summary>
public class SqliteStoringOptions
{
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
    public IRetryStrategy DeleteRetry { get; set; } = new ConstantBackoff {Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 5};
}
