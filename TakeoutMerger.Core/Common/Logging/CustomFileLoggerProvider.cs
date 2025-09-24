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
        // Ensure that only information level and higher logs are recorded
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        // Ensure that only information level and higher logs are recorded
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Get the formatted log message
        var message = formatter(state, exception);

        //Write log messages to text file
        logFileWriter.WriteLine($"[{logLevel}] [{categoryName}] {message}");
        logFileWriter.Flush();
    }
}