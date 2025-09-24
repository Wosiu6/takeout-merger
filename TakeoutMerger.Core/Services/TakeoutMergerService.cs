using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Handlers.Directories;
using TakeoutMerger.Core.Handlers.Files;

namespace TakeoutMerger.Core.Services;

public interface ITakeoutMergerService
{
    Task StandardizeJsonNamesAsync(string inputFolder);
    Task ApplyTakeoutMetadata(string inputFolder, string outputFolder);
    void EnsureWorkingDirectoryExists(string inputFolder, string outputFolder);
}

public class TakeoutMergerService(ILogger<TakeoutMergerService> logger, IJsonNameStandardizationHandler jsonNameStandardizationHandler, IDirectoryHandler  directoryHandler)
    : ITakeoutMergerService
{
    private readonly ILogger _logger = logger;
    private readonly IJsonNameStandardizationHandler _jsonNameStandardizationHandler = jsonNameStandardizationHandler;
    private readonly IDirectoryHandler _directoryHandler = directoryHandler;

    public async Task StandardizeJsonNamesAsync(string inputFolder)
    {
        _logger.LogInformation("Standardizing json names");

        await _jsonNameStandardizationHandler.HandleAsync(inputFolder);

        _logger.LogInformation("Standardized json names");
    }

    public async Task ApplyTakeoutMetadata(string inputFolder, string outputFolder)
    {
        _logger.LogInformation("Applying take out metadata");

        var allDirectories = Directory.GetDirectories(inputFolder, "*", SearchOption.AllDirectories);

        foreach (var directory in allDirectories)
        {
            await _directoryHandler.HandleAsync(directory, outputFolder);
        }
        
        await _directoryHandler.HandleAsync(inputFolder, outputFolder);
            
        _logger.LogInformation("Applied take out metadata");
    }

    public void EnsureWorkingDirectoryExists(string inputFolder, string outputFolder)
    {
        DirectoryUtils.EnsureWorkingDirectoryExists(inputFolder, outputFolder);
    }
}