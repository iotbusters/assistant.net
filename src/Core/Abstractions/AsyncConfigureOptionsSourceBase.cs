using System.Threading.Tasks;

namespace Assistant.Net.Abstractions;

/// <summary>
///     Base asynchronous implementation of configure options source.
/// </summary>
public abstract class AsyncConfigureOptionsSourceBase<TOptions> : ConfigureOptionsSourceBase<TOptions>
    where TOptions : class
{
    /// <inheritdoc/>
    public override void Configure(TOptions options) =>
        ConfigureAsync(options).ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    ///     Configures a <typeparamref name="TOptions"/> instance asynchronously.
    /// </summary>
    protected abstract Task ConfigureAsync(TOptions options);
}