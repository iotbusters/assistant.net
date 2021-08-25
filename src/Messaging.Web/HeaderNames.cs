namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Http header names.
    /// </summary>
    public static class HeaderNames
    {
        /// <summary>
        ///     Defines a header for identification of requested message.
        /// </summary>
        public const string MessageName = "message-name";

        /// <summary>
        ///     Defines a header for correlation of requested message.
        /// </summary>
        public const string CorrelationId = "correlation-id";
    }
}
