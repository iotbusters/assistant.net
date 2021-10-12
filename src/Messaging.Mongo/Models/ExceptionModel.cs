using System;

namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     Exception serialization presentation.
    /// </summary>
    /// <remarks>
    ///     It is just a container replacing original exception object for Bson serialization
    ///     as BSon serializer isn't able properly handle exceptions (e.g. message is being lost).
    /// </remarks>
    public class ExceptionModel
    {
        /// <summary>
        ///     <see cref="Exception.GetType"/> string representation.
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>
        ///     <see cref="Exception.Message"/> representation.
        /// </summary>
        public string Message { get; set; } = default!;

        /// <summary>
        ///     <see cref="Exception.InnerException"/> representation.
        /// </summary>
        public ExceptionModel? InnerException { get; set; } = default!;
    }
}
