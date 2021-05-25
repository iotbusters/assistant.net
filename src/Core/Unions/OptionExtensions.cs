using System;
using Assistant.Net.Unions;

namespace Assistant.Net.Unions
{
    public static class OptionExtensions
    {
        public static Option<TOut> Map<TIn, TOut>(this Option<TIn> option, Func<TIn, TOut> mapper) => option switch
        {
            Some<TIn>(var value) => Option.Some(mapper(value)),
            _                    => Option.None
        };
    }
}