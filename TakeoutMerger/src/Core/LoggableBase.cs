using Microsoft.Extensions.Logging;

namespace TakeoutMerger.src.Core
{
    public abstract class LoggableBase
    {
        public ILogger Logger { get; init; }

        public LoggableBase(ILogger logger)
        {
            Logger = logger;
            Logger.BeginScope(GetType().Name);
        }
    }
}
