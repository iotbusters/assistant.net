using Assistant.Net.Diagnostics.Abstractions;
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
    internal class MessageHandlingService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IOptions<MongoHandlingServerOptions> handlerOptions;
        private readonly IMongoRecordReader recordReader;

        public MessageHandlingService(
            IServiceScopeFactory scopeFactory,
            IOptions<MongoHandlingServerOptions> handlerOptions,
            IMongoRecordReader recordReader)
        {
            this.scopeFactory = scopeFactory;
            this.handlerOptions = handlerOptions;
            this.recordReader = recordReader;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var options = handlerOptions.Value;
                await foreach (var record in recordReader.FindRequested(token))
                {
                    using var scope = scopeFactory.CreateScope();
                    var provider = scope.ServiceProvider;

                    var diagnosticContext = provider.GetRequiredService<InternalDiagnosticContext>();
                    diagnosticContext.CorrelationId = record.Properties.CorrelationId;

                    var processor = provider.GetRequiredService<IMongoRecordProcessor>();
                    var recordWriter = provider.GetRequiredService<IMongoRecordWriter>();

                    if (await processor.Process(record, token) is Some<MongoRecord>(var updated))
                        await recordWriter.Update(updated, token);

                    await Task.Delay(options.NextMessageDelayTime, token);
                }

                await Task.Delay(options.InactivityDelayTime, token);
            }
        }
    }
}
