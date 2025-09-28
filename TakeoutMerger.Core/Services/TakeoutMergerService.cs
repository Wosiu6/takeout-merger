using Microsoft.Extensions.Logging;
using Spectre.Console;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Handlers.Directories;
using TakeoutMerger.Core.Handlers.Files;
using ZLogger;

namespace TakeoutMerger.Core.Services;

public interface ITakeoutMergerService
{
    Task StandardizeJsonNamesAsync(string inputFolder);
    Task ApplyTakeoutMetadata(string inputFolder, string outputFolder);
    void EnsureWorkingDirectoryExists(string inputFolder, string outputFolder);
}

public class TakeoutMergerService(
    ILogger<TakeoutMergerService> logger,
    IJsonNameStandardizationHandler jsonNameStandardizationHandler,
    IDirectoryHandler directoryHandler)
    : ITakeoutMergerService
{
    private readonly ILogger _logger = logger;
    private readonly IJsonNameStandardizationHandler _jsonNameStandardizationHandler = jsonNameStandardizationHandler;
    private readonly IDirectoryHandler _directoryHandler = directoryHandler;

    public async Task StandardizeJsonNamesAsync(string inputFolder)
    {
        _logger.ZLogInformation($"Standardizing json names");

        await _jsonNameStandardizationHandler.HandleAsync(inputFolder);

        _logger.ZLogInformation($"Standardized json names");
    }

    public async Task ApplyTakeoutMetadata(string inputFolder, string outputFolder)
    {
        _logger.ZLogInformation($"Applying take out metadata");

        var allDirectories = Directory.GetDirectories(inputFolder, "*", SearchOption.AllDirectories);
        var allFiles = FileUtils.GetFilesExceptExtensions(inputFolder, [".json"], SearchOption.AllDirectories);
        var totalItems = allFiles.Count();

        await AnsiConsole
            .Progress() // temporary progress bar, should be moved out and sorted out as a separate mechanism
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn()
            })
            .StartAsync(async ctx =>
            {
                var progressTask = ctx.AddTask($"Overall Takeout Processing done", new ProgressTaskSettings
                {
                    MaxValue = totalItems
                });

                foreach (var directory in allDirectories)
                {
                    await Task.Run(async () =>
                    {
                        await _directoryHandler.HandleAsync(directory, outputFolder, progressTask);
                    });
                }

                await _directoryHandler.HandleAsync(inputFolder, outputFolder, progressTask);
            });

        _logger.ZLogInformation($"Applied take out metadata");
    }

    public void EnsureWorkingDirectoryExists(string inputFolder, string outputFolder)
    {
        DirectoryUtils.EnsureWorkingDirectoryExists(inputFolder, outputFolder);
    }
}