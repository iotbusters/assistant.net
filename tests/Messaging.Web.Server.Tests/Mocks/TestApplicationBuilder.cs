using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    public class TestApplicationBuilder : IApplicationBuilder
    {
        public int Count { get; private set; }

        public IServiceProvider ApplicationServices { get; set; } = new ServiceCollection().BuildServiceProvider();

        public IFeatureCollection ServerFeatures => throw new NotImplementedException();

        public IDictionary<string, object?> Properties => throw new NotImplementedException();

        public RequestDelegate Build() => throw new NotImplementedException();

        public IApplicationBuilder New() => throw new NotImplementedException();

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            Count++;
            return this;
        }
    }
}