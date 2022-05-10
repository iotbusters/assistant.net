using Assistant.Net.Storage.Models;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Storage.Internal;

/// <summary>
///     
/// </summary>
public static class DetailExtensions
{
    /// <summary>
    ///     
    /// </summary>
    public static IEnumerable<Detail> ToDetailArray(this IDictionary<string, string> details) =>
        details.Select(x => new Detail(x.Key, x.Value)).ToList();

    /// <summary>
    ///     
    /// </summary>
    public static IDictionary<string, string> ToDictionary(this IEnumerable<Detail> details) =>
        details.ToDictionary(x => x.Name, x => x.Value);
}
