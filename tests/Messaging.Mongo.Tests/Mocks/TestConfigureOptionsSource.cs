using Assistant.Net.Abstractions;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Mongo.Tests.Mocks;

public class TestConfigureOptionsSource<TOptions> : ConfigureOptionsSourceBase<TOptions> where TOptions : class
{
    public override void Configure(TOptions options)
    {
        foreach (var configure in Configurations)
            configure(options);
    }

    public List<Action<TOptions>> Configurations { get; } = new();
}
