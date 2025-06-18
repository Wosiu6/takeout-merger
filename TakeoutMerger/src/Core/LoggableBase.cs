using Microsoft.Extensions.Logging;
using System.Reflection;

namespace TakeoutMerger.src.Core
{
    public abstract class LoggableBase
    {
        public ILogger Logger { get; init; }

        public LoggableBase(ILogger logger)
        {
            Logger = logger;
            Logger.BeginScope(GetType().BaseType.Name);
        }
    }
}
