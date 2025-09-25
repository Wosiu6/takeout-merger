using ZLogger;
// ReSharper disable RedundantLambdaParameterType, This is due to unsupported feature in non-preview version

namespace TakeoutMerger.Logging;

public static class ZLoggerExtensions
{
    public static ZLoggerOptions UseCustomPlainTextFormatter(this ZLoggerOptions options)
    {
        return options.UsePlainTextFormatter(formatter =>
        {
            formatter.SetPrefixFormatter($"[{0:yyyy-MM-dd HH:mm:ss.fff}|{1,-7}|{2}] ",
                (in MessageTemplate template, in LogInfo info) => template.Format(
                    info.Timestamp,
                    info.LogLevel,
                    Environment.CurrentManagedThreadId));

            formatter.SetSuffixFormatter($" ({0}:{1}:{2})",
                (in MessageTemplate template, in LogInfo info) => template.Format(
                    info.Category,
                    info.FilePath ?? "Unknown",
                    info.LineNumber));

            formatter.SetExceptionFormatter((writer, ex) =>
            {
                if (ex.StackTrace != null)
                    Utf8StringInterpolation.Utf8String.Format(writer,
                        $"{ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
            });
        });
    }
}