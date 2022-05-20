using Assistant.Net.Storage.Models;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Storage.Internal;

/// <summary>
///     <see cref="StorageValueDetail"/> mapping extensions.
/// </summary>
public static class DetailExtensions
{
    /// <summary>
    ///     Converts the <paramref name="details"/> dictionary to list of details.
    /// </summary>
    public static IEnumerable<StorageValueDetail> ToDetailArray(this IDictionary<string, string> details) =>
        details.Select(x => new StorageValueDetail(x.Key, x.Value)).ToList();

    /// <summary>
    ///     Converts list of details to a dictionary.
    /// </summary>
    public static IDictionary<string, string> FromDetailArray(this IEnumerable<StorageValueDetail> details) =>
        details.ToDictionary(x => x.Name, x => x.Value);
}
