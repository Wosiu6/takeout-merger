using Microsoft.Extensions.Logging;

namespace TakeoutMerger.Core.Common.Logging;

public class CustomFileLoggerProvider(StreamWriter logFileWriter) : ILoggerProvider
{
    private readonly StreamWriter _logFileWriter = logFileWriter ?? throw new ArgumentNullException(nameof(logFileWriter));

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomFileLogger(categoryName, _logFileWriter);
    }

    public void Dispose()
    {
        _logFileWriter.Dispose();
    }
}

public class CustomFileLogger(string categoryName, StreamWriter logFileWriter) : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
    {
        return logFileWriter;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (exception != null)
        {
            var message = formatter(state, exception);

            logFileWriter.WriteLine($"[{logLevel}] [{categoryName}] {message}");
        }

        logFileWriter.Flush();
    }
}