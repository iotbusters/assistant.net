using System;

namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     MongoDB message handling coordination record.
    /// </summary>
    public class MongoRecord
    {
        /// <summary/>
        public MongoRecord(string id, string name, object message, object? response, HandlingStatus status, RecordProperties properties)
        {
            Id = id;
            Name = name;
            Message = message;
            Response = response;
            Status = status;
            Properties = properties;
        }

        /// <summary>
        ///     Message unique id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     Message name (or type).
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Message object payload.
        /// </summary>
        public object Message { get; private set; }

        /// <summary>
        ///     Handled message response object payload.
        /// </summary>
        /// <remarks>
        ///     It depends on <see cref="Status"/>:
        ///     <list>
        ///         <item>
        ///             <term><see cref="HandlingStatus.Succeeded"/></term>
        ///             <description>contains requested response object.</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="HandlingStatus.Failed"/></term>
        ///             <description>contains <see cref="ExceptionModel"/> object.</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public object? Response { get; private set; }

        /// <summary>
        ///     Message handling status.
        /// </summary>
        public HandlingStatus Status { get; private set; }

        /// <summary>
        ///     Message handling related data (e.g. audit, correlation id)
        /// </summary>
        public RecordProperties Properties { get; private set; }

        /// <summary>
        ///     Creates a new record based on current with a response object and respective status.
        /// </summary>
        public MongoRecord Succeed(object response, DateTimeOffset updated) =>
            new(Id, Name, Message, response, HandlingStatus.Succeeded, Properties.OnUpdated(updated));

        /// <summary>
        ///     Creates a new record based on current with an exception object and respective status.
        /// </summary>
        public MongoRecord Fail(ExceptionModel response, DateTimeOffset updated) =>
            new(Id, Name, Message, response, HandlingStatus.Failed, Properties.OnUpdated(updated));

        /// <summary>
        ///     Creates a new unresponded record with status requested.
        /// </summary>
        public static MongoRecord Request(string id, object message, string correlationId, DateTimeOffset created) => new(
            id,
            name: message.GetType().Name,
            message,
            response: null,
            HandlingStatus.Requested,
            RecordProperties.OnCreated(correlationId, created));
    }
}
