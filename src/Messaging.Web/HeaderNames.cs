namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Http header names.
    /// </summary>
    public static class HeaderNames
    {
        /// <summary>
        ///     Defines a header for identification of requested command.
        /// </summary>
        public const string CommandName = "command-name";

        /// <summary>
        ///     Defines a header for correlation of requested command.
        /// </summary>
        public const string CorrelationId = "correlation-id";
    }
}
