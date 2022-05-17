using Assistant.Net.Diagnostics.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions;

/// <summary>
///     Operation tracking for remote message handling.
/// </summary>
public class OperationHandler : DelegatingHandler
{
    private readonly IDiagnosticFactory diagnosticFactory;

    /// <summary/>
    public OperationHandler(IDiagnosticFactory diagnosticFactory) =>
        this.diagnosticFactory = diagnosticFactory;

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var messageName = request.GetMessageName().ToLower();
        var operation = diagnosticFactory.Start($"{messageName}-handling-remote-client");

        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch
        {
            operation.Fail();
            throw;
        }
        finally
        {
            operation.Complete();
        }
    }
}
