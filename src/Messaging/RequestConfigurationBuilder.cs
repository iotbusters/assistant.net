using System;
using System.Collections;
using System.Collections.Generic;
using Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Assistant.Net.Messaging
{
    public class RequestConfigurationBuilder
    {
        public RequestConfigurationBuilder(IServiceCollection services) =>
            Services = services;

        internal IServiceCollection Services { get; }

        public void Add<TInterceptor>() where TInterceptor : class, IInterceptor =>
            Services.TryAddTransient<IInterceptor, TInterceptor>();
    }
}