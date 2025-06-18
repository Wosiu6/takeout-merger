using Microsoft.Extensions.Logging;
using TakeoutMerger.Core;
using TakeoutMerger.Handlers;
using TakeoutMerger.Utils;

namespace TakeoutMerger.Services
{

    public class TagImageService(ILogger logger, string inputPath, string outputPath) : LoggableBase(logger), IFileTypeProcessService
    {
        private readonly string _inputPath = inputPath;
        private readonly string _outputPath = outputPath;

        public void Process()
        {
            IFileService fileService = new FileService(Logger);
            List<string> foundTagTypesPaths = fileService.GetFilesByExtensions(_inputPath, [".tiff", ".jpg", ".jpeg"]);
            Dictionary<string, string>? foundTagTypesTakeoutPairs = fileService.GetFileDataMatches(_inputPath, foundTagTypesPaths);

            IMetaDataApplier metaDataApplier = new MetaDataApplier(Logger);

            int currentProgress = 0;

            foreach (var foundTagTypesTakeoutPair in foundTagTypesTakeoutPairs)
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(foundTagTypesTakeoutPair.Key);
                var fileExtension = Path.GetExtension(foundTagTypesTakeoutPair.Key);

                var newPath = FileUtils.GetUniqueFileName($"{_outputPath}\\{fileNameNoExt}{fileExtension}");
                File.Copy(foundTagTypesTakeoutPair.Key, newPath);

                metaDataApplier.ApplyJsonMetaDataToTagImage(newPath, foundTagTypesTakeoutPair.Value, _outputPath);

                Console.WriteLine("Applying Json to Tags {0}/{1}: {2}",
                                ++currentProgress, foundTagTypesTakeoutPairs.Count, foundTagTypesTakeoutPair.Key);
            }
        }
    }
}
