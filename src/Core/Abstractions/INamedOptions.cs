using Assistant.Net.Options;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Abstractions;

/// <summary>
///     Configured <typeparamref name="TOptions"/> instance based on scoped <see cref="NamedOptionsContext"/>.
/// </summary>
/// <typeparam name="TOptions">The type of options being requested.</typeparam>
public interface INamedOptions<out TOptions> : IOptions<TOptions> where TOptions : class { }
