using Microsoft.Extensions.Logging;

namespace Assistant.Net.Abstractions;

public interface IScopedLogger
{
    void UseScopeProvider(IExternalScopeProvider provider);
}
