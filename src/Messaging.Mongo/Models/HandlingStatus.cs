namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     Message handling status.
    /// </summary>
    public enum HandlingStatus
    {
        /// <summary>
        ///     Message was requested to be handled.
        /// </summary>
        Requested = 1,

        /// <summary>
        ///     Message handling is complete and a successful response is going to be responded.
        /// </summary>
        Succeeded,

        /// <summary>
        ///     Message handling is complete and an exception is going to be responded.
        /// </summary>
        Failed
    }
}
