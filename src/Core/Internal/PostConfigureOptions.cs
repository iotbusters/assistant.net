using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Internal
{
    internal class PostConfigureOptions<TOptions> : IPostConfigureOptions<TOptions> where TOptions : class
    {
        private readonly string name;
        private readonly Action action;

        public PostConfigureOptions(string name, Action action)
        {
            this.name = name;
            this.action = action;
        }

        public void PostConfigure(string name, TOptions options)
        {
            if (this.name != name)
                return;

            action();
        }
    }
}
