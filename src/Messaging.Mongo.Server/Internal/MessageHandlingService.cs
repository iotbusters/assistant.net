using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Message handling orchestrating service.
    /// </summary>
    internal class MessageHandlingService : BackgroundService
    {
        private readonly IOptionsMonitor<MongoHandlingServerOptions> options;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IMongoRecordReader recordReader;

        public MessageHandlingService(
            IOptionsMonitor<MongoHandlingServerOptions> options,
            IServiceScopeFactory scopeFactory,
            IMongoRecordReader recordReader)
        {
            this.options = options;
            this.scopeFactory = scopeFactory;
            this.recordReader = recordReader;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var serverOptions = options.CurrentValue;

                if (await recordReader.NextRequested(token) is not Some<MongoRecord>(var record))
                {
                    await Task.Delay(serverOptions.InactivityDelayTime, token);
                    continue;
                }

                using var scope = scopeFactory.CreateScope();
                var provider = scope.ServiceProvider;

                var diagnosticContext = provider.GetRequiredService<DiagnosticContext>();
                diagnosticContext.CorrelationId = new Audit(record.Details).CorrelationId;

                var processor = provider.GetRequiredService<IMongoRecordProcessor>();
                var recordWriter = provider.GetRequiredService<IMongoRecordWriter>();

                if (await processor.Process(record, token) is Some<MongoRecord>(var updated))
                    await recordWriter.Update(updated, token);

                await Task.Delay(serverOptions.NextMessageDelayTime, token);
            }
        }
    }
}
