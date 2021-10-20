using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     MongoDB message handling coordination record.
    /// </summary>
    public class MongoRecord
    {
        /// <summary/>
        public MongoRecord(string id, string messageName, object message, object? response, HandlingStatus status, Audit audit)
        {
            Id = id;
            MessageName = messageName;
            Message = message;
            Response = response;
            Status = status;
            Details = audit.Details;
        }

        /// <summary/>
        public MongoRecord(string id, string messageName, object message, Audit audit)
            : this(id, messageName, message, response: null, HandlingStatus.Requested, audit) { }

        /// <summary>
        ///     Message unique id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     Message name (or type).
        /// </summary>
        public string MessageName { get; private set; }

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
        ///     Message handling related audit details.
        /// </summary>
        public IDictionary<string, object> Details { get; private set; }

        /// <summary>
        ///     Creates a new record based on current with a response object and respective status.
        /// </summary>
        public MongoRecord Succeed(object response, DateTimeOffset completed) =>
            new(Id, MessageName, Message, response, HandlingStatus.Succeeded, new Audit(Details) {Completed = completed});

        /// <summary>
        ///     Creates a new record based on current with an exception object and respective status.
        /// </summary>
        public MongoRecord Fail(ExceptionModel response, DateTimeOffset completed) =>
            new(Id, MessageName, Message, response, HandlingStatus.Failed, new Audit(Details) {Completed = completed});
    }
}
