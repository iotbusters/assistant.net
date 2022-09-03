using Assistant.Net.Diagnostics.EventSources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Assistant.Net.Diagnostics.Internal;

/// <summary>
///     Common operation over <see cref="Activity"/> object.
/// </summary>
internal static class ActivityExtensions
{
    public const string CorrelationIdName = "correlation-id";
    public const string UserName = "user";
    public const string StatusName = "operation-status";
    public const string MessageName = "operation-message";

    public static readonly ISet<string> KnownNames = new HashSet<string>
    {
        CorrelationIdName, StatusName, MessageName
    };

    /// <summary>
    ///    Adds a current correlation ID to the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static Activity AddCorrelationId(this Activity activity, string correlationId) => activity
        .SetTag(CorrelationIdName, correlationId)
        .TryAddBaggage(CorrelationIdName, correlationId);

    /// <summary>
    ///    Gets a current correlation ID of the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static string GetCorrelationId(this Activity activity) =>
        activity.Tags.SingleOrDefault(x => x.Key == CorrelationIdName).Value
        ?? throw new ArgumentException($"Activity({activity.OperationName}) doesn't have {CorrelationIdName} tag value.");

    /// <summary>
    ///    Adds a current user to the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static Activity AddUser(this Activity activity, string user) => activity
        .SetTag(UserName, user)
        .TryAddBaggage(UserName, user);

    /// <summary>
    ///    Gets a current user of the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static string GetUser(this Activity activity) =>
        activity.Tags.SingleOrDefault(x => x.Key == UserName).Value
        ?? throw new ArgumentException($"Activity({activity.OperationName}) doesn't have {UserName} tag value.");

    /// <summary>
    ///    Gets parent correlation IDs of the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static ItemData[] GetParentCorrelationIds(this Activity activity) =>
        activity.Baggage
            .Where(x => x.Key == CorrelationIdName)
            .Select(x => x.Value!)
            .Except(new[] { activity.GetCorrelationId() })
            .Select(x => new ItemData { Value = x })
            .ToArray();

    /// <summary>
    ///    Gets parent users of the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static ItemData[] GetParentUsers(this Activity activity) =>
        activity.Baggage
            .Where(x => x.Key == UserName)
            .Select(x => x.Value!)
            .Except(new[] {activity.GetUser()})
            .Select(x => new ItemData {Value = x})
            .ToArray();

    /// <summary>
    ///    Adds an operation status message to the activity.
    /// </summary>
    public static Activity AddMessage(this Activity activity, string message) => activity
        .SetTag(MessageName, message);

    /// <summary>
    ///    Gets an operation status message of the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static string GetMessage(this Activity activity) =>
        activity.Tags.SingleOrDefault(x => x.Key == MessageName).Value
        ?? throw new ArgumentException($"Activity({activity.OperationName}) doesn't have {MessageName} tag value.");

    /// <summary>
    ///    Adds an operation status to the activity.
    /// </summary>
    public static Activity AddOperationStatus(this Activity activity, string status) => activity
        .SetTag(StatusName, status);

    /// <summary>
    ///    Gets an operation status of the activity.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static string GetOperationStatus(this Activity activity) =>
        activity.Tags.SingleOrDefault(x => x.Key == StatusName).Value
        ?? throw new ArgumentException($"Activity({activity.OperationName}) doesn't have {StatusName} tag value.");

    /// <summary>
    ///    Gets custom user metadata of the activity.
    /// </summary>
    public static IDictionary<string, ItemData[]> GetCustomMetadata(this Activity activity) =>
        activity.Tags
            .Concat(activity.Baggage)
            .Where(x => !KnownNames.Contains(x.Key))
            .Where(x => x.Value != null)
            .GroupBy(x => x.Key, x => x.Value!)
            .ToDictionary(x => x.Key, x => x.Select(y => new ItemData {Value = y}).ToArray());

    /// <summary>
    ///    Adds new value to cross-activity baggage by a key, if a value doesn't exists.
    ///    Multiple values can be associated to a single key.
    /// </summary>
    internal static Activity TryAddBaggage(this Activity activity, string key, string value) =>
        activity.GetBaggageItem(key) != value
            ? activity.AddBaggage(key, value)
            : activity;
}
