using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Options;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Web.Tests.Mocks
{
    public class TestConfigureOptionsSource : ConfigureOptionsSourceBase<MessagingClientOptions>
    {
        public override void Configure(MessagingClientOptions options)
        {
            foreach (var configure in Configurations)
                configure(options);
        }

        public List<Action<MessagingClientOptions>> Configurations { get; } = new();
    }
}
