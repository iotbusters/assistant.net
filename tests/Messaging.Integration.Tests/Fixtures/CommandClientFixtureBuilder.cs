using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Integration.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Linq;

namespace Assistant.Net.Messaging.Integration.Tests.Fixtures
{
    public class CommandClientFixtureBuilder
    {
        public CommandClientFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddCommandClient(b => b.ClearInterceptors())
                .AddRemoteWebCommandClient(o =>
                {
                    o.BaseAddress = new Uri("http://localhost/command");
                    // debugging purpose only.
                    if (Debugger.IsAttached)
                        o.Timeout = TimeSpan.FromSeconds(300);
                }).Services;
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseRemoteWebCommandHandler()))
                .ConfigureServices(s => s.AddRemoteWebCommandHandler(b => b.ClearInterceptors()));
        }

        public IServiceCollection Services { get; init; }
        public IHostBuilder RemoteHostBuilder { get; init; }

        public CommandClientFixtureBuilder ClearHandlers()
        {
            Services.ConfigureCommandClient(b => b.ClearInterceptors());
            return this;
        }

        public CommandClientFixtureBuilder AddLocal<THandler>() where THandler : class, IAbstractHandler
        {
            Services.ConfigureCommandClient(b => b.AddLocal<THandler>());
            return this;
        }

        public CommandClientFixtureBuilder AddRemote<THandler>() where THandler : class, IAbstractHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureCommandClient(b => b.AddLocal<THandler>()));

            var commandType = typeof(THandler)
                .GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                ?.GetGenericArguments().First()
                ?? throw new ArgumentException("Invalid command handler type.", nameof(THandler));

            Services.ConfigureCommandClient(b => b.AddRemote(commandType));
            return this;
        }

        public CommandClientFixtureBuilder AddRemoteCommandRegistrationOnly<TCommand>()
            where TCommand : IAbstractCommand
        {
            var commandType = typeof(TCommand);
            Services.ConfigureCommandClient(b => b.AddRemote(commandType));
            return this;
        }

        public CommandClientFixture Create()
        {
            var provider = Services
                .AddHttpClientRedirect<IRemoteCommandClient>(_ => RemoteHostBuilder.Start())
                .BuildServiceProvider();
            return new(provider);
        }
    }
}