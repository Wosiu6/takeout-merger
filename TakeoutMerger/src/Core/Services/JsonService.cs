using Microsoft.Extensions.Logging;
using TakeoutMerger.src.Core.Handlers;

namespace TakeoutMerger.src.Core.Services
{
    public class JsonService : LoggableBase, IFileTypeProcessService
    {
        private readonly ILogger _logger;
        private readonly string _inputPath;
        private readonly string _outputPath;
        public JsonService(ILogger logger, string inputPath, string outputPath) : base(logger)
        {
            _logger = logger;
            _inputPath = inputPath;
            _outputPath = outputPath;
        }
        public void Process()
        {
            _logger.LogInformation("Processing JSON files from {InputPath} to {OutputPath}", _inputPath, _outputPath);

            IFileService fileService = new FileService(Logger);
            List<string> foundJsons = fileService.GetFilesByExtensions(_inputPath, [".json"]);

            IJsonNameHandler jsonNameHandler = new JsonNameHandler(Logger);

            HashSet<string> newJsonFiles = [];

            int currentProgress = 0;

            foreach (var foundJson in foundJsons)
            {
                var newJsonFile = jsonNameHandler.GenerateNewJsonFile(foundJson, _outputPath);
                newJsonFiles.Add(newJsonFile);

                Logger.LogInformation("Applying Json to PNGs {0}/{1}: {2}",
                                ++currentProgress, foundJsons.Count, foundJson);
            }
        }
    }
}
