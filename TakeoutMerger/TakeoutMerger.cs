using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Logging;
using TakeoutMerger.Core.Services;

namespace TakeoutMerger;

public class TakeoutMerger()
{
    private int _counter = 0;
    private int _amountOfFolders = 0;

    public async Task StartAsync(string[] args)
    {
        var outputPath = args[0];
        var inputPath = args[1];
        
#if DEBUG
        string logFilePath = $"{outputPath}\\console_log.txt";
        using StreamWriter logFileWriter = new(logFilePath, append: true);
        ILogger logger = SetupLogger(logFileWriter);
#else
            ILogger logger = SetupLogger();
#endif
        SetupFirstChanceException(logger);
        
        var subDirectories = Directory.GetDirectories(inputPath, "*", SearchOption.AllDirectories);
        _amountOfFolders = subDirectories.Length + 1; // +1 for the top directory itself

        List<Task> subDirectoryTasks = [];

        foreach (var subDirectory in subDirectories)
        {
            var task = ProcessFolderAsync(logger, subDirectory, outputPath);

            subDirectoryTasks.Add(task);
        }

        var topDirectoryTask =
            ProcessFolderAsync(logger, inputPath, outputPath, searchOption: SearchOption.TopDirectoryOnly);
        subDirectoryTasks.Add(topDirectoryTask);

        await Task.WhenAll(subDirectoryTasks);
        
        logger.LogInformation("Takeout Merger concluded successfully, processed: {AmountOfFiles} folders", _amountOfFolders);
    }

    private async Task ProcessFolderAsync(ILogger logger, string inputPath, string outputPath,
        SearchOption searchOption = SearchOption.AllDirectories)
    {
        try
        {
            logger.LogInformation("Processing folder: {InputPath}", inputPath);

            JsonService jsonService = new(logger, inputPath, outputPath, searchOption);
            await jsonService.ProcessAsync();

            PngService pngService = new(logger, inputPath, outputPath, searchOption);
            await pngService.ProcessAsync();

            TagImageService tagImageService = new(logger, inputPath, outputPath, searchOption);
            await tagImageService.ProcessAsync();

            NonExifFilesService nonExifFilesService = new(logger, inputPath, outputPath, searchOption);
            await nonExifFilesService.ProcessAsync();

            Interlocked.Increment(ref _counter);
            logger.LogCritical("Progress: {Counter}/{AmountOfFolders}.", _counter, _amountOfFolders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing folder: {InputPath}", inputPath);
        }
    }

    private static ILogger SetupLogger(StreamWriter? logFileWriter = null)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            builder
#if DEBUG
                .AddProvider(new CustomFileLoggerProvider(logFileWriter!))
#endif
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }));

        return loggerFactory.CreateLogger("TakeoutMerger");
    }

    private static void SetupFirstChanceException(ILogger logger)
    {
        AppDomain.CurrentDomain.FirstChanceException += FirstChanceHandler;
        
        void FirstChanceHandler(object source, FirstChanceExceptionEventArgs e)
        {
            logger.LogError("FirstChanceException: {ExceptionDetails}", e.Exception);
        }
    }
}