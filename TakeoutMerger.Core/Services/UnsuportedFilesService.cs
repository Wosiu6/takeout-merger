using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.Handlers;
using TakeoutMerger.Core.Services.Interfaces;

namespace TakeoutMerger.Core.Services;

public class UnsuportedFilesService(ILogger logger, string inputPath, string outputPath, SearchOption searchOption = SearchOption.AllDirectories) : LoggableBase(logger), IFileTypeProcessService
{
    private readonly string _inputPath = inputPath;
    private readonly string _outputPath = outputPath;
    private readonly SearchOption _searchOption = searchOption;

    public void Process()
    {
        IFileService fileService = new FileService(Logger);
        List<string> foundUnsuportedPaths = fileService.GetFilesByExtensions(_inputPath, [".tiff", ".jpg", ".jpeg", ".png", ".json"], excludeExtensions: true, searchOption: _searchOption);

        if (foundUnsuportedPaths.Count == 0)
        {
            Logger.LogWarning("No unsupported files found in the specified directory.");
            return;
        }

        Dictionary<string, string>? foundTagTypesTakeoutPairs = fileService.GetFileDataMatches(_inputPath, foundUnsuportedPaths);

        IMetaDataApplier metaDataApplier = new MetaDataApplier(Logger);

        int currentProgress = 0;

        foreach (var foundUnsuportedPath in foundUnsuportedPaths)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(foundUnsuportedPath);
            var fileExtension = Path.GetExtension(foundUnsuportedPath);

            var newPath = FileUtils.GetUniqueFileName($"{_outputPath}\\{fileNameNoExt}{fileExtension}");

            File.Copy(foundUnsuportedPath, newPath);

            Logger.LogInformation("Copying movies {0}/{1}: {2}",
                ++currentProgress, foundUnsuportedPaths.Count, foundUnsuportedPath);
        }
    }
}