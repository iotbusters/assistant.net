using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Conversion between <see cref="Exception"/> and its representation <see cref="ExceptionModel"/>
    ///     for Bson serialization purpose only.
    /// </summary>
    public class ExceptionModelConverter
    {
        private readonly ITypeEncoder typeEncoder;
        private readonly IOptions<MessagingClientOptions> options;

        /// <summary/>
        public ExceptionModelConverter(ITypeEncoder typeEncoder, IOptions<MessagingClientOptions> options)
        {
            this.typeEncoder = typeEncoder;
            this.options = options;
        }

        /// <summary>
        ///     Converts <paramref name="exception"/> into <see cref="ExceptionModel"/>.
        /// </summary>
        public ExceptionModel? ConvertTo(Exception? exception)
        {
            if (exception == null)
                return null;

            var type = exception.GetType();
            if (exception is not MessageException && !options.Value.ExposedExceptions.Any(x => x.IsAssignableFrom(type)))
                return null;

            var typeName = typeEncoder.Encode(type) ?? throw NotSupportedTypeException(type);

            return new ExceptionModel {Type = typeName, Message = exception.Message, InnerException = ConvertTo(exception.InnerException)};
        }

        /// <summary>
        ///     Converts <paramref name="model"/> into <see cref="Exception"/>.
        /// </summary>
        public Exception? ConvertFrom(ExceptionModel? model)
        {
            if (model == null)
                return null;

            var type = typeEncoder.Decode(model.Type) ?? throw NotSupportedTypeException(model.Type);

            return (Exception)Activator.CreateInstance(type, model.Message, ConvertFrom(model.InnerException))!;
        }

        private static NotSupportedException NotSupportedTypeException(string type) =>
            new($"Type '{type}' isn't supported.");

        private static NotSupportedException NotSupportedTypeException(Type type) =>
            new($"Type '{type.Name}' isn't supported.");
    }
}
