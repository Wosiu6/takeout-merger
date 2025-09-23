using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Handlers;
using TakeoutMerger.Core.Services.Interfaces;

namespace TakeoutMerger.Core.Services;

public class NonExifFilesService(ILogger logger, string inputPath, string outputPath, SearchOption searchOption = SearchOption.AllDirectories) : LoggableBase(logger), IFileTypeProcessService
{
    private readonly string _inputPath = inputPath;
    private readonly string _outputPath = outputPath;
    private readonly SearchOption _searchOption = searchOption;

    public async Task ProcessAsync()
    {
        IFileService fileService = new FileService(Logger);
        List<string> foundNonExifPaths = fileService.GetFilesByExtensions(_inputPath, [".tiff", ".jpg", ".jpeg", ".png", ".json"], excludeExtensions: true, searchOption: _searchOption);

        if (foundNonExifPaths.Count == 0)
        {
            Logger.LogWarning("No nonExif files found in the specified directory.");
            return;
        }

        Dictionary<string, string>? foundTagTypesTakeoutPairs = fileService.GetFileDataMatches(_inputPath, foundNonExifPaths);

        IMetaDataApplier metaDataApplier = new MetaDataApplier(Logger);

        int currentProgress = 0;

        foreach (var foundNonExifTakeoutPair in foundTagTypesTakeoutPairs)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(foundNonExifTakeoutPair.Key);
            var fileExtension = Path.GetExtension(foundNonExifTakeoutPair.Key);

            var newPath = $"{outputPath}\\{fileNameNoExt}{fileExtension}";
            newPath = FileUtils.GetUniqueFilePath(newPath);
            
            File.Copy(foundNonExifTakeoutPair.Key, newPath, true);
            
            metaDataApplier.ApplyJsonMetaDataToNonExifFile(newPath, foundNonExifTakeoutPair.Value, _outputPath);

            Logger.LogInformation("Applying Json to Non Exif {0}/{1}: {2}",
                ++currentProgress, foundNonExifPaths.Count, newPath);
        }
    }
}