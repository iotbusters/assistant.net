﻿using Assistant.Net.Storage.Options;

namespace Assistant.Net.Storage;

/// <summary>
///     MongoDB options extensions.
/// </summary>
public static class MongoOptionsExtensions
{
    /// <summary>
    ///     Configures MongoDB connection string.
    /// </summary>
    public static MongoOptions Connection(this MongoOptions options, string connectionString)
    {
        options.ConnectionString = connectionString;
        return options;
    }

    /// <summary>
    ///     Configures MongoDB database name.
    /// </summary>
    public static MongoOptions Database(this MongoOptions options, string databaseName)
    {
        options.DatabaseName = databaseName;
        return options;
    }

    /// <summary>
    ///     Configures MongoDB database initialization.
    /// </summary>
    public static MongoOptions EnsureCreated(this MongoOptions options, bool ensureDatabaseCreated = true)
    {
        options.EnsureDatabaseCreated = ensureDatabaseCreated;
        return options;
    }
}
