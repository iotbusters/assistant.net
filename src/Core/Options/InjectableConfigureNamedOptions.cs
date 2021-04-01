using System;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Options
{
    public class InjectableConfigureNamedOptions<TOptions> : ConfigureNamedOptions<TOptions> where TOptions : class
    {
        public InjectableConfigureNamedOptions(string name, IServiceProvider provider, Action<IServiceProvider, TOptions> action)
            : base(name, options => action(provider, options))
        {
        }
    }
}