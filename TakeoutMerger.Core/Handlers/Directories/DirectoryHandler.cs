using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Handlers.Files;
using TakeoutMerger.Core.Handlers.Metadata;
using ZLogger;
using Spectre.Console;

namespace TakeoutMerger.Core.Handlers.Directories;

public interface IDirectoryHandler
{
    Task HandleAsync(string directory, string outputFolder, ProgressTask progressTask);
}

public class DirectoryHandler(
    ILogger<DirectoryHandler> logger,
    IFileHandler fileHandler,
    IFileMetadataMatcher fileMetadataMatcher) : IDirectoryHandler
{
    private readonly IFileMetadataMatcher _fileMetadataMatcher = fileMetadataMatcher;
    private readonly IFileHandler _fileHandler = fileHandler;
    private readonly ILogger _logger = logger;

    public async Task HandleAsync(string directory, string outputFolder, ProgressTask progressTask)
    {
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(outputFolder))
        {
            throw new ArgumentException("Directory and output folder paths cannot be empty");
        }

        _logger.ZLogInformation($"Processing directory: {directory}");

        var fileJsonPairs = _fileMetadataMatcher.GetFileDataMatches(directory);

        foreach (var pair in fileJsonPairs)
        {
            await _fileHandler.HandleAsync(pair.Key, pair.Value, outputFolder);
            progressTask.Increment(1);
        }

        _logger.ZLogInformation($"Completed processing {fileJsonPairs.Count} files in {directory}");
    }
}