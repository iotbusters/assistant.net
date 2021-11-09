﻿using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Mongo options extensions for MongoDb based remote message handling.
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
    }
}