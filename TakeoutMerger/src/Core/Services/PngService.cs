using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using TakeoutMerger.src.Core;
using TakeoutMerger.src.Core.Converters;
using TakeoutMerger.src.Core.Handlers;

namespace TakeoutMerger.src.Core.Services
{
    public class PngService(ILogger logger, string inputPath, string outputPath, SearchOption searchOption = SearchOption.AllDirectories) : LoggableBase(logger), IFileTypeProcessService
    {
        private readonly string _inputPath = inputPath;
        private readonly string _outputPath = outputPath;
        private readonly SearchOption _searchOption = searchOption;

        public void Process()
        {
            IFileService fileService = new FileService(Logger);
            List<string> foundPngPaths = fileService.GetFilesByExtensions(_inputPath, [".png"], searchOption: _searchOption);

            if (foundPngPaths.Count == 0)
            {
                Logger.LogWarning("No PNG files found in the specified directory.");
                return;
            }

            IDictionary<string, string> pngTakeoutPairs = fileService.GetFileDataMatches(_inputPath, foundPngPaths).ToFrozenDictionary();

            IMetaDataApplier metaDataApplier = new MetaDataApplier(Logger);

            int currentProgress = 0;

            foreach (var pngTakeoutPair in pngTakeoutPairs)
            {
                var newPath = new PngToTiffConverter(Logger, _outputPath).Convert(pngTakeoutPair.Key, CompressionMode.None);

                var newNameNoExtension = Path.GetFileNameWithoutExtension(newPath);

                metaDataApplier.ApplyJsonMetaDataToPng(newPath, pngTakeoutPair.Value, _outputPath);

                Logger.LogInformation("Applying Json to PNGs {0}/{1}: {2}",
                                ++currentProgress, pngTakeoutPairs.Count, pngTakeoutPair.Key);
            }
        }
    }
}
