using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Web.Tests.Mocks;

public class TestApplicationBuilder : IApplicationBuilder
{
    public int Count { get; private set; }

    public IServiceProvider ApplicationServices { get; set; } = new ServiceCollection()
        .AddLogging()
        .AddRouting()
        .AddHealthChecks().Services
        .Configure<HealthCheckServiceOptions>(delegate { })
        .BuildServiceProvider();

    public IFeatureCollection ServerFeatures => throw new NotImplementedException();

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    public RequestDelegate Build() => _ => throw new NotImplementedException();

    public IApplicationBuilder New() => this;

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        Count++;
        return this;
    }
}
