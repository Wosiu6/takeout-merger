using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using TakeoutMerger.Converters;
using TakeoutMerger.Core;
using TakeoutMerger.Handlers;

namespace TakeoutMerger.Services
{
    public class PngService(ILogger logger, string inputPath, string outputPath) : LoggableBase(logger), IFileTypeProcessService
    {
        private readonly string _inputPath = inputPath;
        private readonly string _outputPath = outputPath;

        public void Process()
        {
            IFileService fileService = new FileService(Logger);
            List<string> foundPngPaths = fileService.GetFilesByExtensions(_inputPath, [".png"]);
            IDictionary<string, string> pngTakeoutPairs = fileService.GetFileDataMatches(_inputPath, foundPngPaths).ToFrozenDictionary();

            IMetaDataApplier metaDataApplier = new MetaDataApplier(Logger);

            int currentProgress = 0;

            foreach (var pngTakeoutPair in pngTakeoutPairs)
            {
                var newPath = PngToTiffConverter.Convert(pngTakeoutPair.Key, CompressionMode.None);

                var newNameNoExtension = Path.GetFileNameWithoutExtension(newPath);

                metaDataApplier.ApplyJsonMetaDataToPng(newPath, pngTakeoutPair.Value, _outputPath);

                Logger.LogInformation("Applying Json to PNGs {0}/{1}: {2}",
                                ++currentProgress, pngTakeoutPairs.Count, pngTakeoutPair.Key);
            }
        }
    }
}
