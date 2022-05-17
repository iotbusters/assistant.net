using Microsoft.Extensions.Primitives;

namespace Assistant.Net.Abstractions;

/// <summary>
///     Tracking options configuration abstraction.
/// </summary>
/// <typeparam name="TOptions">Options type.</typeparam>
public interface IConfigureOptionsSource<in TOptions> where TOptions : class
{
    /// <summary>
    ///     Gets <see cref="IChangeToken"/> used for tracking options changes.
    /// </summary>
    IChangeToken GetChangeToken();

    /// <summary>
    ///     Configures a <typeparamref name="TOptions"/> instance.
    /// </summary>
    void Configure(TOptions options);
}