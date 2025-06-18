using Microsoft.Extensions.Logging;

namespace TakeoutMerger.Core
{
    public abstract class LoggableBase
    {
        public ILogger Logger { get; init; }

        public LoggableBase(ILogger logger)
        {
            Logger = logger;
            Logger.BeginScope(this.GetType().Name);
        }
    }
}
