using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;

namespace Assistant.Net.Internal
{
    internal class NamedOptionsChangeTokenSource<TOptions> : IOptionsChangeTokenSource<TOptions>
    {
        private readonly Func<IChangeToken> getter;

        public NamedOptionsChangeTokenSource(string name, Func<IChangeToken> getter)
        {
            this.getter = getter;
            Name = name;
        }

        public IChangeToken GetChangeToken() => getter();

        public string Name { get; }
    }
}
