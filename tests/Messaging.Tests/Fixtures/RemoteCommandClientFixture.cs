// using System;
// using Assistant.Net.Messaging.Abstractions;
// using Assistant.Net.Messaging.Internal;
// using Assistant.Net.Messaging.Options;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.TestHost;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// ;

// namespace Assistant.Net.Messaging.Tests.Fixtures
// {
//     public class RemoteCommandClientFixture : IDisposable
//     {
//         public RemoteCommandClientFixture(Action<CommandOptions> configureCommandOptions)
//         {
//             TestHost = new HostBuilder()
//                 .ConfigureWebHost(wb => wb
//                     .UseTestServer()
//                     .Configure(b => b.UseRemoteCommandHandling())
//                     .ConfigureServices(s => s
//                         .AddSystemServicesHosted()
//                         .AddRemoteCommandHandlingServer(configureCommandOptions)))
//                 .Start();
//             Client = new ServiceCollection()
//                 .AddRemoteCommandHandlingClient(o => o.BaseAddress = new Uri("http://localhost/command"))
//                 .AddCommandClient(o => o.Handlers.AddRemote<TestCommand1>())
//                 .AddHttpClientRedirect<RemoteCommandHandlingClient>(host)
//                 .BuildServiceProvider()
//                 .GetRequiredService<ICommandClient>();
//         }

//         protected IHost? TestHost { get; private set; }

//         public ICommandClient? Client { get; set; }

//         public void Dispose()
//         {
//             var host = TestHost ?? throw new ObjectDisposedException(nameof(TestHost));
//             TestHost = null;
//             Client = null;

//             host.Dispose();
//             GC.SuppressFinalize(this);
//         }
//     }
// }