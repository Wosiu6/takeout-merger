using Microsoft.Extensions.Logging;
using TakeoutMerger.src.Common.Utils;
using TakeoutMerger.src.Core.Services;

namespace TakeoutMerger.src
{
    public class TakeoutMerger
    {
        public void Start(string[] args)
        {
            ILogger logger = SetupLogger();

            if (args.Length < 1)
            {
                logger.LogError("Insufficient parameters supplied.");
                logger.LogInformation("Usage: TakeoutMerger <directoryPath> <outputPath>");
                logger.LogInformation("Example: TakeoutMerger C:\\Downloads\\Takeout C:\\Downloads\\MyOutputFolder");
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];

            DirectoryUtils.EnsureWorkingDirectoryExists(inputPath, outputPath, logger);


            JsonService jsonService = new(logger, inputPath, outputPath);
            jsonService.Process();

            return;
            PngService pngService = new(logger, inputPath, outputPath);
            pngService.Process();

            TagImageService tagImageService = new(logger, inputPath, outputPath);
            tagImageService.Process();

            UnsuportedFilesService unsuportedFilesService = new(logger, inputPath, outputPath);
            unsuportedFilesService.Process();
        }

        private static ILogger SetupLogger()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }));

            return loggerFactory.CreateLogger("TakeoutMerger");
        }
    }
}
