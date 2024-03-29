﻿using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Storage.Options;

/// <summary>
///     MongoDB client configuration for specific provider usage.
/// </summary>
public sealed class MongoOptions
{
    /// <summary>
    ///     MongoDB server connection string.
    /// </summary>
    [Required, MinLength(10)]//10:  mongodb://
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    ///     Database name.
    /// </summary>
    [Required, MinLength(1)]
    public string DatabaseName { get; set; } = null!;

    /// <summary>
    ///     Determine if database should be configured before use.
    /// </summary>
    public bool EnsureDatabaseCreated { get; set; } = false;
}
