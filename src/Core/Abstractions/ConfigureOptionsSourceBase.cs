using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Threading;

namespace Assistant.Net.Abstractions
{
    /// <summary>
    ///     Base implementation of configure options source.
    /// </summary>
    public abstract class ConfigureOptionsSourceBase<TOptions> : IConfigureOptionsSource<TOptions>
        where TOptions : class
    {
        private ConfigurationReloadToken changeToken = new();

        /// <inheritdoc/>
        public IChangeToken GetChangeToken() => changeToken;

        /// <inheritdoc/>
        public abstract void Configure(TOptions options);

        /// <summary>
        ///     Triggers the change token reload.
        /// </summary>
        public void Reload() => Interlocked.Exchange(ref changeToken, new()).OnReload();
    }
}
