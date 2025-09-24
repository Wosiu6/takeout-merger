using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Handlers.Files;

namespace TakeoutMerger.Core.Handlers.Directories;

public interface IDirectoryHandler
{
    Task HandleAsync(string directory, string outputFolder);
}

public class DirectoryHandler(ILogger<DirectoryHandler> logger, IFileHandler fileHandler, IFileMetadataMatcher fileMetadataMatcher) : IDirectoryHandler
{
    private readonly IFileMetadataMatcher _fileMetadataMatcher = fileMetadataMatcher;
    private readonly IFileHandler _fileHandler = fileHandler;
    private readonly ILogger _logger = logger;
    
    public async Task HandleAsync(string directory, string outputFolder)
    {
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(outputFolder))
        {
            throw new ArgumentException("Directory and output folder paths cannot be empty");
        }

        _logger.LogInformation("Processing directory: {DirectoryPath}", directory);

        var fileJsonPairs = _fileMetadataMatcher.GetFileDataMatches(directory);
        
        foreach (var pair in fileJsonPairs)
        {
            await _fileHandler.HandleAsync(pair.Key, pair.Value, outputFolder);
        }

        _logger.LogInformation("Completed processing {FileCount} files in {DirectoryPath}", 
            fileJsonPairs.Count, directory);
    }
}