using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Assistant.Net.Diagnostics.Internal
{
    /// <summary>
    ///     Common operation over <see cref="Activity" /> object.
    /// </summary>
    internal static class ActivityExtensions
    {
        public const string CorrelationIdName = "correlation-id";
        public const string StatusName = "operation-status";
        public const string MessageName = "operation-message";

        public static ISet<string> KnownNames = new HashSet<string>
        {
            CorrelationIdName, StatusName, MessageName
        };

        public static Activity AddCorrelationId(this Activity activity, string correlationId)
        {
            return activity
                .AddTag(CorrelationIdName, correlationId)
                .TryAddBaggage(CorrelationIdName, correlationId);
        }

        public static string GetCorrelationId(this Activity activity) =>
            activity.Tags.LastOrDefault(x => x.Key == CorrelationIdName).Value
            ?? throw new ArgumentException($"Activity({activity.OperationName}) doesn't have {CorrelationIdName} baggage value.");

        public static Activity AddMessage(this Activity activity, string message) =>
            activity.SetTag(MessageName, message);

        public static string GetMessage(this Activity activity) =>
            activity.Tags.LastOrDefault(x => x.Key == MessageName).Value
            ?? throw new ArgumentException($"Activity({activity.OperationName}) doesn't have {MessageName} tag value.");

        public static Activity AddOperationStatus(this Activity activity, string status) =>
            activity.SetTag(StatusName, status);

        public static string GetOperationStatus(this Activity activity) =>
            activity.Tags.LastOrDefault(x => x.Key == StatusName).Value
            ?? throw new ArgumentException($"Activity({activity.OperationName}) doesn't have {StatusName} tag value.");

        public static IDictionary<string, string> GetCustomMetadata(this Activity activity) =>
            activity.Tags
                .Where(x => !KnownNames.Contains(x.Key))
                .Concat(activity.Baggage)
                .Where(x => x.Value != null)
                .GroupBy(x => x.Key, x => x.Value!)
                .ToDictionary(x => x.Key, x => string.Join(",", x));

        internal static Activity TryAddBaggage(this Activity activity, string key, string value) =>
            activity.GetBaggageItem(key) != value
                ? activity.AddBaggage(key, value)
                : activity;
    }
}