using Microsoft.Extensions.Logging;
using TakeoutMerger.src.Core.Handlers;

namespace TakeoutMerger.src.Core.Services
{
    public class JsonService(ILogger logger, string inputPath, string outputPath, SearchOption searchOption = SearchOption.AllDirectories) : LoggableBase(logger), IFileTypeProcessService
    {
        private readonly ILogger _logger = logger;
        private readonly string _inputPath = inputPath;
        private readonly string _outputPath = outputPath;
        private readonly SearchOption _searchOption = searchOption;

        public void Process()
        {
            _logger.LogInformation("Processing JSON files int {InputPath}", _inputPath);

            IFileService fileService = new FileService(Logger);
            List<string> foundJsons = fileService.GetFilesByExtensions(_inputPath, [".json"], searchOption: _searchOption);

            if (foundJsons.Count == 0)
            {
                Logger.LogInformation("No JSON files found in {InputPath}", _inputPath);
                return;
            }

            IJsonNameHandler jsonNameHandler = new JsonNameHandler(Logger);

            int currentProgress = 0;

            foreach (var foundJson in foundJsons)
            {
                var newJsonFile = jsonNameHandler.GenerateNewJsonFile(foundJson, _outputPath);

                File.Delete(foundJson);

                Logger.LogInformation("Generating new json {0}/{1}: {2}",
                                ++currentProgress, foundJsons.Count, foundJson);
            }
        }
    }
}
