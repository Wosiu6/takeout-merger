using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Logging;
using TakeoutMerger.Core.Handlers;
using TakeoutMerger.Core.Services.Interfaces;

namespace TakeoutMerger.Core.Services;

public class JsonService(ILogger logger, string inputPath, string outputPath, SearchOption searchOption = SearchOption.AllDirectories) : LoggableBase(logger), IFileTypeProcessService
{
    private readonly string _inputPath = inputPath;
    private readonly string _outputPath = outputPath;
    private readonly SearchOption _searchOption = searchOption;

    public void Process()
    {
        Logger.LogInformation("Processing JSON files int {InputPath}", _inputPath);

        IFileService fileService = new FileService(Logger);
        List<string> foundJsons = fileService.GetFilesByExtensions(_inputPath, [".json"], searchOption: _searchOption);

        if (foundJsons.Count == 0)
        {
            Logger.LogInformation("No JSON files found in {InputPath}", _inputPath);
            return;
        }

        JsonNameHandler jsonNameHandler = new (Logger);

        int currentProgress = 0;

        foreach (string foundJsonName in foundJsons)
        {
            string newJsonFile = jsonNameHandler.GenerateNewJsonFile(foundJsonName, _outputPath);

            File.Delete(foundJsonName);

            Logger.LogInformation("Generating new json {CurrentProgress}/{FoundJsonsCount}: {FoundJsonName}",
                ++currentProgress, foundJsons.Count, foundJsonName);
        }
    }
}