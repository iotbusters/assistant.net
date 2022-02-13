using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;

namespace Assistant.Net.Options;

/// <summary>
///     Lambda based <see cref="IOptionsChangeTokenSource{TOptions}"/> implementation.
/// </summary>
/// <typeparam name="TOptions">The options type being tracked.</typeparam>
public class LambdaOptionsChangeTokenSource<TOptions> : IOptionsChangeTokenSource<TOptions>
{
    private readonly Func<IChangeToken> getter;

    /// <summary/>
    public LambdaOptionsChangeTokenSource(string name, Func<IChangeToken> getter)
    {
        this.getter = getter;
        Name = name;
    }

    /// <inheritdoc />
    public IChangeToken GetChangeToken() => getter();

    /// <inheritdoc />
    public string Name { get; }
}
