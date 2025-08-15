
using Microsoft.Extensions.Logging;

namespace TakeoutMerger.Core.Common.Logging;

public abstract class LoggableBase
{
    public ILogger Logger { get; init; }

    public LoggableBase(ILogger logger)
    {
        Logger = logger;
        Logger.BeginScope(GetType().Name);
    }
}