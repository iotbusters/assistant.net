using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging.Tests.Fixtures
{
    public class CommandClientFixtureBuilder
    {
        public CommandClientFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddCommandClient()
                .AddRemoteCommandHandlingClient(o => o.BaseAddress = new Uri("http://localhost/command"));
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseRemoteCommandHandling())
                .ConfigureServices(s => s.AddSystemServicesHosted().AddJsonSerializerOptions()));
        }

        public IServiceCollection Services { get; init; }
        public IHostBuilder RemoteHostBuilder { get; init; }

        public CommandClientFixtureBuilder AddLocal<THandler>() where THandler : class, IAbstractCommandHandler
        {
            Services.AddCommandOptions(o => o.Handlers.AddLocal<THandler>());
            return this;
        }

        public CommandClientFixtureBuilder AddRemote<THandler>() where THandler : class, IAbstractCommandHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .AddRemoteCommandHandlingServer(o => o.Handlers.AddLocal<THandler>()));

            var commandType = typeof(THandler)
                .GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                ?.GetGenericArguments().First()
                ?? throw new ArgumentException("Invalid command handler type.", nameof(THandler));

            Services.AddCommandOptions(options => options.Handlers.AddRemote(commandType));
            return this;
        }

        public CommandClientFixtureBuilder AddRemoteCommandRegistrationOnly<TCommand>()
            where TCommand : IAbstractCommand
        {
            var commandType = typeof(TCommand);
            Services.AddCommandOptions(options => options.Handlers.AddRemote(commandType));
            return this;
        }

        public CommandClientFixture Create()
        {
            var provider = Services
                .AddHttpClientRedirect<RemoteCommandHandlingClient>(p => RemoteHostBuilder.Start())
                .BuildServiceProvider();
            return new(provider);
        }
    }
}