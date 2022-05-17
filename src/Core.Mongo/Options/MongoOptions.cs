﻿using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Options
{
    /// <summary>
    ///     MongoDB client configuration for specific provider usage.
    /// </summary>
    public class MongoOptions
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
        public string DatabaseName { get; set; } = "Data";
    }
}
