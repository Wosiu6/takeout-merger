using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Handlers;
using TakeoutMerger.Core.Services.Interfaces;

namespace TakeoutMerger.Core.Services;

public class TagImageService(ILogger logger, string inputPath, string outputPath, SearchOption searchOption = SearchOption.AllDirectories) : LoggableBase(logger), IFileTypeProcessService
{
    private readonly string _inputPath = inputPath;
    private readonly string _outputPath = outputPath;
    private readonly SearchOption _searchOption = searchOption;

    public async Task ProcessAsync()
    {
        IFileService fileService = new FileService(Logger);
        List<string> foundTagTypesPaths = fileService.GetFilesByExtensions(_inputPath, [".tiff", ".jpg", ".jpeg"], _searchOption);

        if (foundTagTypesPaths.Count == 0)
        {
            Logger.LogWarning("No Tag Type files found in the specified directory.");
            return;
        }

        Dictionary<string, string>? foundTagTypesTakeoutPairs = fileService.GetFileDataMatches(_inputPath, foundTagTypesPaths);

        IMetaDataApplier metaDataApplier = new MetaDataApplier(Logger);

        int currentProgress = 0;

        foreach (var foundTagTypesTakeoutPair in foundTagTypesTakeoutPairs)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(foundTagTypesTakeoutPair.Key);
            var fileExtension = Path.GetExtension(foundTagTypesTakeoutPair.Key);

            var newPath = FileUtils.GetUniqueFilePath($"{_outputPath}\\{fileNameNoExt}{fileExtension}");

            File.Copy(foundTagTypesTakeoutPair.Key, newPath, true);
            
            metaDataApplier.ApplyJsonMetaDataToTagImage(newPath, foundTagTypesTakeoutPair.Value, _outputPath);

            File.Delete(foundTagTypesTakeoutPair.Key);
            
            Logger.LogInformation("Applying Json to Tags {CurrentProgress}/{TagTypeCount}: {Key}",
                ++currentProgress, foundTagTypesTakeoutPairs.Count, foundTagTypesTakeoutPair.Key);
        }
    }
}