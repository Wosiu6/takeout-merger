using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Logging;
using TakeoutMerger.Core.Converters;
using TakeoutMerger.Core.Handlers;
using TakeoutMerger.Core.Services.Interfaces;

namespace TakeoutMerger.Core.Services;

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

            Logger.LogInformation("Applying Json to PNGs {CurrentProgress}/{PngPairCount}: {Key}",
                ++currentProgress, pngTakeoutPairs.Count, pngTakeoutPair.Key);
        }
    }
}