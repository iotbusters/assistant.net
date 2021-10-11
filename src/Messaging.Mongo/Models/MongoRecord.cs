using System;

namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     
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
        ///     
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public object Message { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public object? Response { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public HandlingStatus Status { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public RecordProperties Properties { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public MongoRecord Succeed(object response, DateTimeOffset updated) =>
            new(Id, Name, Message, response, HandlingStatus.Succeeded, Properties.OnUpdated(updated));

        /// <summary>
        ///     
        /// </summary>
        public MongoRecord Fail(Exception response, DateTimeOffset updated) =>
            new(Id, Name, Message, response, HandlingStatus.Failed, Properties.OnUpdated(updated));

        /// <summary>
        ///     
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
