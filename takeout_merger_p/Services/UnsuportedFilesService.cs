using Microsoft.Extensions.Logging;
using TakeoutMerger.Core;
using TakeoutMerger.Handlers;
using TakeoutMerger.Utils;

namespace TakeoutMerger.Services
{

    public class UnsuportedFilesService(ILogger logger, string inputPath, string outputPath) : LoggableBase(logger), IFileTypeProcessService
    {
        private readonly string _inputPath = inputPath;
        private readonly string _outputPath = outputPath;

        public void Process()
        {
            IFileService fileService = new FileService(Logger);
            List<string> foundUnsuportedPaths = fileService.GetFilesByExtensions(_inputPath, [".tiff", ".jpg", ".jpeg", ".png", ".json"], exclude: true);
            Dictionary<string, string>? foundTagTypesTakeoutPairs = fileService.GetFileDataMatches(_inputPath, foundUnsuportedPaths);

            IMetaDataApplier metaDataApplier = new MetaDataApplier(Logger);

            int currentProgress = 0;

            foreach (var foundUnsuportedPath in foundUnsuportedPaths)
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(foundUnsuportedPath);
                var fileExtension = Path.GetExtension(foundUnsuportedPath);

                var newPath = FileUtils.GetUniqueFileName($"{_outputPath}\\{fileNameNoExt}{fileExtension}");

                File.Copy(foundUnsuportedPath, newPath);

                Console.WriteLine("Copying movies {0}/{1}: {2}",
                                ++currentProgress, foundUnsuportedPaths.Count, foundUnsuportedPath);
            }
        }
    }
}
