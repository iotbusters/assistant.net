﻿using System;
using System.Linq;
using System.Net.Http;

namespace Assistant.Net.Messaging.Extensions
{
    internal static  class HttpRequestMessageExtensions
    {
        public static string GetRequiredHeader(this HttpRequestMessage request, string name)
        {
            if (!request.Headers.TryGetValues(name, out var values) || !values.Any())
                throw new InvalidOperationException($"Header '{name}' is required.");

            return values.First();
        }

        public static void SetHeader(this HttpRequestMessage request, string name, string value) =>
            request.Headers.Add(name, value);

        public static string GetCommandName(this HttpRequestMessage request) =>
            request.GetRequiredHeader(HeaderNames.CommandName);

        public static void SetCorrelationId(this HttpRequestMessage request, Guid correlationId) =>
            request.SetHeader(HeaderNames.CorrelationIdName, correlationId.ToString());
    }
}